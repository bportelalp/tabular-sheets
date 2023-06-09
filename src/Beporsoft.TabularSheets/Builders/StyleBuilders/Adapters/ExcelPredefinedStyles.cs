﻿using Beporsoft.TabularSheets.CellStyling;
using Beporsoft.TabularSheets.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Beporsoft.TabularSheets.Builders.StyleBuilders.Adapters
{
    /// <summary>
    /// A class to encapsullate the default styles used by Microsoft Excel which is required to correct rendering.
    /// </summary>
    internal class ExcelPredefinedStyles
    {
        internal List<FillSetup> PredefinedFills { get; set; } = new();
        internal List<FontSetup> PredefinedFonts { get; set; } = new();
        internal List<BorderSetup> PredefinedBorders { get; set; } = new();
        internal List<FormatSetup> PredefinedFormats { get; set; } = new();
        internal List<NumberingFormatSetup> PredefinedNumberingFormats { get; set; } = new();

        /// <summary>
        /// Create an instance of <see cref="ExcelPredefinedStyles"/> with all predefined styles demanded by MS Excel
        /// </summary>
        internal static ExcelPredefinedStyles Create()
        {
            ExcelPredefinedStyles defaults = new ExcelPredefinedStyles();

            // Predefined Fills
            FillSetup fillNone = new FillSetup(new FillStyle() { PatternValue = DocumentFormat.OpenXml.Spreadsheet.PatternValues.None });
            FillSetup fillGray125 = new FillSetup(new FillStyle() { PatternValue = DocumentFormat.OpenXml.Spreadsheet.PatternValues.Gray125 });
            defaults.PredefinedFills.Add(fillNone);
            defaults.PredefinedFills.Add(fillGray125);

            // Predefined Borders
            BorderSetup borderEmpty = new BorderSetup(new BorderStyle());
            defaults.PredefinedBorders.Add(borderEmpty);

            // Predefined Numbering Format
            NumberingFormatSetup numFormatGeneral = new NumberingFormatSetup("General");
            defaults.PredefinedNumberingFormats.Add(numFormatGeneral);

            // Predefined Font
            FontSetup fontCalibri12 = new FontSetup(new FontStyle() { Font = KnownFont.Calibri, Size = 12 });
            defaults.PredefinedFonts.Add(fontCalibri12);

            // Predefined Format
            FormatSetup formatEmpty = new FormatSetup(fillNone, fontCalibri12, borderEmpty, numFormatGeneral);
            defaults.PredefinedFormats.Add(formatEmpty);

            return defaults;
        }
    }
}
