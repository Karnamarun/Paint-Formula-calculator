using System.Collections.ObjectModel;
using System.Windows.Input;
using PaintTintCalculator.Wpf.Commands;
using PaintTintCalculator.Wpf.Models;
using PaintTintCalculator.Wpf.Services;

namespace PaintTintCalculator.Wpf.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly IApiClient _apiClient;

    private string _searchText = string.Empty;
    private ShadeModel? _selectedShade;
    private BaseModel? _selectedBase;
    private CanSizeOption? _selectedCanSize;

    private decimal _totalColorantMl;
    private decimal _tintPercent;
    private decimal _maxTintPercent;
    private decimal _baseCost;
    private decimal _colorantCost;
    private decimal _totalPrice;

    private string _currencySymbol = "₹";
    private bool _isConnected;
    private string _connectionStatusText = "Connecting...";

    private bool _isLoading;
    private string _loadingMessage = "Loading...";

    private string? _errorMessage;
    private string? _successMessage;
    private int _lastJobId;

    public ObservableCollection<ShadeModel> Shades { get; } = new();
    public ObservableCollection<BaseModel> Bases { get; } = new();
    public ObservableCollection<CanSizeOption> CanSizes { get; } = new();
    public ObservableCollection<FormulaRowModel> FormulaRows { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set
        {
            if (SetProperty(ref _searchText, value))
            {
                // Auto-trigger search when user clears or types
                _ = SearchShadesAsync();
            }
        }
    }

    public ShadeModel? SelectedShade
    {
        get => _selectedShade;
        set
        {
            if (SetProperty(ref _selectedShade, value))
            {
                ClearMessages();
                _ = RecalculateAsync();
            }
        }
    }

    public BaseModel? SelectedBase
    {
        get => _selectedBase;
        set
        {
            if (SetProperty(ref _selectedBase, value))
            {
                ClearMessages();
                _ = RecalculateAsync();
            }
        }
    }

    public CanSizeOption? SelectedCanSize
    {
        get => _selectedCanSize;
        set
        {
            if (SetProperty(ref _selectedCanSize, value))
            {
                ClearMessages();
                _ = RecalculateAsync();
            }
        }
    }

    public decimal TotalColorantMl { get => _totalColorantMl; private set => SetProperty(ref _totalColorantMl, value); }
    public decimal TintPercent { get => _tintPercent; private set => SetProperty(ref _tintPercent, value); }
    public decimal MaxTintPercent { get => _maxTintPercent; private set => SetProperty(ref _maxTintPercent, value); }
    public decimal BaseCost { get => _baseCost; private set => SetProperty(ref _baseCost, value); }
    public decimal ColorantCost { get => _colorantCost; private set => SetProperty(ref _colorantCost, value); }
    public decimal TotalPrice { get => _totalPrice; private set => SetProperty(ref _totalPrice, value); }

    public string CurrencySymbol
    {
        get => _currencySymbol;
        set => SetProperty(ref _currencySymbol, value);
    }

    public bool IsConnected
    {
        get => _isConnected;
        private set => SetProperty(ref _isConnected, value);
    }

    public string ConnectionStatusText
    {
        get => _connectionStatusText;
        private set => SetProperty(ref _connectionStatusText, value);
    }

    public bool IsLoading
    {
        get => _isLoading;
        private set
        {
            if (SetProperty(ref _isLoading, value))
            {
                OnPropertyChanged(nameof(CanDispense));
            }
        }
    }

    public string LoadingMessage
    {
        get => _loadingMessage;
        private set => SetProperty(ref _loadingMessage, value);
    }

    public string? ErrorMessage
    {
        get => _errorMessage;
        private set
        {
            if (SetProperty(ref _errorMessage, value))
            {
                OnPropertyChanged(nameof(HasError));
            }
        }
    }

    public bool HasError => !string.IsNullOrWhiteSpace(_errorMessage);

    public string? SuccessMessage
    {
        get => _successMessage;
        private set
        {
            if (SetProperty(ref _successMessage, value))
            {
                OnPropertyChanged(nameof(HasSuccess));
            }
        }
    }

    public bool HasSuccess => !string.IsNullOrWhiteSpace(_successMessage);

    public int LastJobId
    {
        get => _lastJobId;
        private set => SetProperty(ref _lastJobId, value);
    }

    public bool CanDispense => !IsLoading && SelectedShade != null && SelectedBase != null && SelectedCanSize != null && !HasError && TotalPrice > 0;

    // Commands
    public ICommand SearchCommand { get; }
    public ICommand ClearSearchCommand { get; }
    public ICommand DispenseCommand { get; }
    public ICommand DismissNotificationCommand { get; }
    public ICommand RefreshConnectionCommand { get; }

    public MainViewModel() : this(null)
    {
    }

    public MainViewModel(IApiClient? apiClient)
    {
        _apiClient = apiClient ?? new ApiClient();

        foreach (var option in CanSizeOption.DefaultOptions)
        {
            CanSizes.Add(option);
        }
        _selectedCanSize = CanSizes.FirstOrDefault(s => s.Litres == 4.0m) ?? CanSizes.FirstOrDefault();

        SearchCommand = new AsyncRelayCommand(SearchShadesAsync);
        ClearSearchCommand = new RelayCommand(() => SearchText = string.Empty);
        DispenseCommand = new AsyncRelayCommand(DispenseAsync, () => CanDispense);
        DismissNotificationCommand = new RelayCommand(ClearMessages);
        RefreshConnectionCommand = new AsyncRelayCommand(InitializeAsync);

        _ = InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        LoadingMessage = "Connecting to Paint Tint API...";
        ErrorMessage = null;

        try
        {
            IsConnected = await _apiClient.CheckConnectionAsync();
            ConnectionStatusText = IsConnected ? "API Connected" : "API Offline";

            var bases = await _apiClient.GetBasesAsync();
            Bases.Clear();
            foreach (var b in bases)
            {
                Bases.Add(b);
            }
            if (Bases.Count > 0)
            {
                _selectedBase = Bases[0];
                OnPropertyChanged(nameof(SelectedBase));
            }

            await SearchShadesAsync();
        }
        catch (Exception ex)
        {
            IsConnected = false;
            ConnectionStatusText = "API Offline";
            ErrorMessage = $"Unable to connect to server: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task SearchShadesAsync()
    {
        try
        {
            var results = await _apiClient.GetShadesAsync(SearchText);
            Shades.Clear();
            foreach (var s in results)
            {
                Shades.Add(s);
            }

            if (SelectedShade == null || !Shades.Any(s => s.Id == SelectedShade.Id))
            {
                SelectedShade = Shades.FirstOrDefault();
            }
            else
            {
                await RecalculateAsync();
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to search shades: {ex.Message}";
        }
    }

    public async Task RecalculateAsync()
    {
        if (SelectedShade == null || SelectedBase == null || SelectedCanSize == null)
        {
            FormulaRows.Clear();
            ResetMetrics();
            return;
        }

        IsLoading = true;
        LoadingMessage = "Calculating formula...";
        ErrorMessage = null;

        try
        {
            var result = await _apiClient.CalculateTintAsync(
                SelectedShade.Id,
                SelectedBase.Id,
                SelectedCanSize.Litres);

            if (result.IsSuccess)
            {
                FormulaRows.Clear();
                foreach (var item in result.Items)
                {
                    FormulaRows.Add(item);
                }

                TotalColorantMl = result.TotalColorantMl;
                TintPercent = result.TintPercent;
                MaxTintPercent = result.MaxTintPercent;
                BaseCost = result.BaseCost;
                ColorantCost = result.ColorantCost;
                TotalPrice = result.TotalPrice;
                ErrorMessage = null;
            }
            else
            {
                FormulaRows.Clear();
                ResetMetrics();
                ErrorMessage = result.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            FormulaRows.Clear();
            ResetMetrics();
            ErrorMessage = $"Calculation failed: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
            OnPropertyChanged(nameof(CanDispense));
        }
    }

    public async Task DispenseAsync()
    {
        if (!CanDispense || SelectedShade == null || SelectedBase == null || SelectedCanSize == null)
        {
            return;
        }

        IsLoading = true;
        LoadingMessage = "Dispensing and recording job...";
        ClearMessages();

        try
        {
            var response = await _apiClient.CreateDispenseJobAsync(
                SelectedShade.Id,
                SelectedBase.Id,
                SelectedCanSize.Litres);

            if (response.IsSuccess)
            {
                LastJobId = response.JobId;
                SuccessMessage = $"✓ Dispense saved successfully. Job #{response.JobId} recorded for {SelectedShade.Name} ({SelectedCanSize.DisplayText}).";
            }
            else
            {
                ErrorMessage = response.ErrorMessage;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = $"Failed to record dispense job: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void ResetMetrics()
    {
        TotalColorantMl = 0m;
        TintPercent = 0m;
        MaxTintPercent = SelectedBase?.MaxTintPercent ?? 0m;
        BaseCost = 0m;
        ColorantCost = 0m;
        TotalPrice = 0m;
    }

    public void ClearMessages()
    {
        ErrorMessage = null;
        SuccessMessage = null;
    }
}
