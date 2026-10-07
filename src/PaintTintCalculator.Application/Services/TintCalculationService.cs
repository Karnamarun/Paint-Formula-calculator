using PaintTintCalculator.Application.Abstractions.Services;
using PaintTintCalculator.Application.DTOs.Tint;
using PaintTintCalculator.Domain.Entities;
using PaintTintCalculator.Domain.Exceptions;
using PaintTintCalculator.Domain.Rules;
using PaintTintCalculator.Domain.ValueObjects;

namespace PaintTintCalculator.Application.Services;

public class TintCalculationService : ITintCalculationService
{
    public TintCalculationResultDto Calculate(Shade shade, Base baseEntity, decimal canSizeLitres)
    {
        // 1. Validate can size using domain value object
        var canSize = CanSize.Create(canSizeLitres);

        // 2. Filter formula items for the selected base
        var formulaItems = shade.FormulaItems
            .Where(f => f.BaseId == baseEntity.Id)
            .ToList();

        if (formulaItems.Count == 0)
        {
            throw new FormulaNotFoundException(shade.Name, baseEntity.Name);
        }

        // 3. Scale and round each colorant
        var calculationItems = new List<TintCalculationItemDto>();
        decimal totalColorantMl = 0m;
        decimal totalColorantCost = 0m;

        foreach (var item in formulaItems)
        {
            var dispensedMl = TintCalculationRules.CalculateDispensedMl(item.MlPerLitre, canSize.Litres);
            var colorantCost = PricingRules.CalculateColorantCost(dispensedMl, item.Colorant.CostPerMl);
            var itemCostRounded = Math.Round(colorantCost, 2, MidpointRounding.AwayFromZero);

            totalColorantMl += dispensedMl;
            totalColorantCost += colorantCost;

            calculationItems.Add(new TintCalculationItemDto(
                item.ColorantId,
                item.Colorant.Code,
                item.Colorant.Name,
                item.MlPerLitre,
                dispensedMl,
                item.Colorant.CostPerMl,
                itemCostRounded));
        }

        // 4. Calculate tint percentage
        var tintPercent = TintCalculationRules.CalculateTintPercent(totalColorantMl, canSize.Litres);

        // 5. Enforce maximum tint limit
        TintCalculationRules.ValidateTintLimit(
            baseEntity.Name,
            canSize.Litres,
            totalColorantMl,
            baseEntity.MaxTintPercent);

        // 6. Pricing calculations
        var baseCost = PricingRules.CalculateBaseCost(baseEntity.PricePerLitre, canSize.Litres);
        var baseCostRounded = Math.Round(baseCost, 2, MidpointRounding.AwayFromZero);
        var totalColorantCostRounded = Math.Round(totalColorantCost, 2, MidpointRounding.AwayFromZero);
        var totalPrice = PricingRules.CalculateTotalPrice(baseCost, totalColorantCost);

        return new TintCalculationResultDto(
            shade.Id,
            shade.Code,
            shade.Name,
            shade.HexColor,
            baseEntity.Id,
            baseEntity.Name,
            baseEntity.PricePerLitre,
            canSize.Litres,
            calculationItems,
            totalColorantMl,
            tintPercent,
            baseEntity.MaxTintPercent,
            baseCostRounded,
            totalColorantCostRounded,
            totalPrice);
    }
}

