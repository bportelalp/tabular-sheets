﻿using Beporsoft.TabularSheets.Builders.StyleBuilders;
using Beporsoft.TabularSheets.CellStyling;
using Beporsoft.TabularSheets.Tools;
using DocumentFormat.OpenXml.Spreadsheet;
using System;

namespace Beporsoft.TabularSheets.Builders.SheetBuilders
{
    /// <summary>
    /// The class which handles the creation of the <see cref="SheetData"/> which represent the <see cref="Table"/> and register
    /// the shared elements like shared strings and styles inside the containers <see cref="SharedStrings"/> and <see cref="StyleBuilder"/>
    /// respectively.
    /// </summary>
    internal class SheetBuilder<T>
    {
        private readonly CellRefIterator _cellRefIterator = new CellRefIterator();

        public SheetBuilder(TabularSheet<T> table, StylesheetBuilder styleBuilder, SharedStringBuilder sharedStrings)
        {
            Table = table;
            StyleBuilder = styleBuilder;
            SharedStrings = sharedStrings;
            CellBuilder = new CellBuilder(SharedStrings);
        }

        #region Properties

        /// <summary>
        /// The <see cref="TabularSheet{T}"/> used to build the <see cref="SheetData"/>
        /// </summary>
        public TabularSheet<T> Table { get; }

        /// <summary>
        /// The container which stores the generated styles
        /// </summary>
        public StylesheetBuilder StyleBuilder { get; }

        /// <summary>
        /// The container which stores the shared strings
        /// </summary>
        public SharedStringBuilder SharedStrings { get; }

        /// <summary>
        /// The cell builder, which helps on building <see cref="Cell"/> instances
        /// </summary>
        public CellBuilder CellBuilder { get; }
        #endregion

        #region Public
        /// <summary>
        /// Build the <see cref="SheetData"/> node using values from <see cref="Table"/>. <br/>
        /// In addition, handles the required styles and include it inside the <see cref="StyleBuilder"/>
        /// for after treatment
        /// </summary>
        /// <returns>An instance of the OpenXml element <see cref="SheetData"/></returns>
        public SheetData BuildSheetData()
        {
            SheetData sheetData = new();
            _cellRefIterator.Reset();
            Row header = CreateHeaderRow();
            sheetData.AppendChild(header);

            foreach (var item in Table.Items)
            {
                _cellRefIterator.MoveNextRow();
                Row row = CreateItemRow(item);
                sheetData.AppendChild(row);
            }
            return sheetData;
        }

        public static Columns BuildColumns(TabularSheet<T> table)
        {
            Columns columns = new Columns();
            int index = 1;
            foreach (var colSheet in table.Columns)
            {
                Column col = new Column();
                col.BestFit = true;
                col.Min = index.ToOpenXmlUInt32();
                col.Max = index.ToOpenXmlUInt32();
                columns.Append(col);
                index++;
            }
            return columns;
        }
        #endregion

        #region Create SheetData components
        /// <summary>
        /// Create the <see cref="Row"/> which represents the header of the table, including the registry of
        /// styles inside <see cref="StyleBuilder"/>
        /// </summary>
        private Row CreateHeaderRow()
        {
            Row header = new()
            {
                RowIndex = (_cellRefIterator.CurrentRow + 1).ToOpenXmlUInt32()
            };

            int? formatId = RegisterHeaderStyle();

            _cellRefIterator.ResetCol();
            foreach (var col in Table.Columns)
            {
                Cell cell = CellBuilder.CreateCell(col.Title);
                if (formatId is not null)
                    cell.StyleIndex = formatId.Value.ToOpenXmlUInt32();

                cell.CellReference = _cellRefIterator.MoveNextColAfter();

                header.Append(cell);
            }
            return header;
        }

        /// <summary>
        /// Create the <see cref="Row"/> which represents one item of <typeparamref name="T"/> of the table, including the registry of
        /// styles inside <see cref="StyleBuilder"/>
        /// </summary>
        private Row CreateItemRow(T item)
        {
            Row row = new Row();
            row.RowIndex = (_cellRefIterator.CurrentRow + 1).ToOpenXmlUInt32();
            _cellRefIterator.ResetCol();
            foreach (var col in Table.Columns)
            {
                Cell cell = BuildDataCell(item, col);
                cell.CellReference = _cellRefIterator.MoveNextColAfter();
                row.Append(cell);
            }
            return row;
        }

        /// <summary>
        /// Create the <see cref="Cell"/> filled with <paramref name="item"/> applying the combined style based on the different rules of the table
        /// </summary>
        private Cell BuildDataCell(T item, TabularDataColumn<T> col)
        {
            // Populate with value
            object? value = col.Apply(item);
            Cell cell = new();
            if (value is not null)
                cell = CellBuilder.CreateCell(value);

            // Populate with style
            int? formatId = BuildCellStyle(value, col);
            cell.StyleIndex = formatId.ToOpenXmlUInt32();
            return cell;
        }

        private int? BuildCellStyle(object? value, TabularDataColumn<T> column)
        {
            Style combinedStyle = StyleCombiner.Combine(column.Style, Table.BodyStyle);
            FillSetup? fillSetup = combinedStyle.Fill.Equals(FillStyle.Default) ? null : new FillSetup(combinedStyle.Fill);
            FontSetup? fontSetup = combinedStyle.Font.Equals(FillStyle.Default) ? null : new FontSetup(combinedStyle.Font);
            BorderSetup? borderSetup = combinedStyle.Border.Equals(FillStyle.Default) ? null : new BorderSetup(combinedStyle.Border);
            NumberingFormatSetup? numFmtSetup =
                string.IsNullOrWhiteSpace(combinedStyle.NumberingPattern) is true ? null : new NumberingFormatSetup(combinedStyle.NumberingPattern!);

            if (value is not null && value.GetType() == typeof(DateTime) && numFmtSetup is null)
                numFmtSetup = new NumberingFormatSetup(Table.Options.DateTimeFormat);

            if (fillSetup is null && fontSetup is null && borderSetup is null && numFmtSetup is null)
                return null;

            int formatId = StyleBuilder.RegisterFormat(fillSetup, fontSetup, borderSetup, numFmtSetup);
            return formatId;
        }

        /// <summary>
        /// Configure and register one <see cref="FormatSetup"/> in the <see cref="StyleBuilder"/> to format the heading cells.
        /// </summary>
        /// <returns>The index of the setup, or null if there aren't any style to build</returns>
        private int? RegisterHeaderStyle()
        {
            Style headerStyle = Table.HeaderStyle;

            if (Table.Options.InheritHeaderStyleFromBody)
                headerStyle = StyleCombiner.Combine(Table.HeaderStyle, Table.BodyStyle);

            FillSetup? fill = headerStyle.Fill.Equals(FillStyle.Default) ? null : new FillSetup(headerStyle.Fill);
            FontSetup? font = headerStyle.Font.Equals(FillStyle.Default) ? null : new FontSetup(headerStyle.Font);
            BorderSetup? border = headerStyle.Border.Equals(FillStyle.Default) ? null : new BorderSetup(headerStyle.Border);

            if (font is null && fill is null && border is null)
                return null;

            int formatId = StyleBuilder.RegisterFormat(fill, font, border);

            return formatId;
        }
        #endregion
    }
}
