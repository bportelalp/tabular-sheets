﻿using Beporsoft.TabularSheets.Builders.StyleBuilders;
using Beporsoft.TabularSheets.Helpers;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Beporsoft.TabularSheets.Builders
{
    internal class SheetBuilder<T>
    {
        public SheetBuilder(TabularSpreadsheet<T> table, StylesheetBuilder styleBuilder)
        {
            Table = table;
            StyleBuilder = styleBuilder;
        }

        public TabularSpreadsheet<T> Table { get; }
        public StylesheetBuilder StyleBuilder { get; }


        /// <summary>
        /// Build the <see cref="SheetData"/> node using values from <see cref="Table"/>. <br/>
        /// In addition, handles the required styles and include it inside the <see cref="StyleBuilder"/>
        /// for after treatment
        /// </summary>
        /// <returns>An instance of the OpenXml element <see cref="SheetData"/></returns>
        public SheetData BuildSheetData()
        {
            SheetData sheetData = new SheetData();
            Row header = CreateHeader();
            sheetData.AppendChild(header);

            foreach (var item in Table.Items)
            {
                Row row = CreateRow(item);
                sheetData.AppendChild(row);
            }
            return sheetData;
        }

        /// <summary>
        /// Create the <see cref="Row"/> which represents the header of the table, including the registry of
        /// styles inside <see cref="StyleBuilder"/>
        /// </summary>
        private Row CreateHeader()
        {
            Row header = new Row();
            System.Drawing.Color colorHeader = Table.Header.Color;
            int formatId = StyleBuilder.RegisterFormat(colorHeader);
            foreach (var col in Table.Columns)
            {
                Cell cell = CreateCell(col.Title, CellValues.String);
                cell.StyleIndex = OpenXMLHelpers.ToUint32Value(formatId);
                header.Append(cell);
            }
            return header;
        }

        /// <summary>
        /// Create the <see cref="Row"/> which represents one item of <see cref="{T}"/> of the table, including the registry of
        /// styles inside <see cref="StyleBuilder"/>
        /// </summary>
        private Row CreateRow(T item)
        {
            Row row = new Row();
            foreach (var col in Table.Columns)
            {
                object value = col.Apply(item);
                Cell cell = new Cell();
                if (value is not null)
                    cell = CreateCell(value);

                // Apply StyleIndex
                row.Append(cell);
            }
            return row;
        }

        protected static Cell CreateCell(string value, CellValues dataType)
        {
            return new Cell()
            {
                CellValue = new CellValue(value),
                DataType = new EnumValue<CellValues>(dataType)
            };
        }

        protected static Cell CreateCell(object value)
        {
            Type type = value.GetType();
            Cell cell = new Cell();

            if (type == typeof(string))
            {
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(Convert.ToString(value) ?? string.Empty);
            }
            else if (type == typeof(int))
            {
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(Convert.ToInt32(value));
            }
            else if (type == typeof(double))
            {
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(Convert.ToDouble(value));
            }
            else if (type == typeof(float))
            {
                cell.DataType = CellValues.Number;
                cell.CellValue = new CellValue(Convert.ToDecimal(value));
            }
            else if (type == typeof(bool))
            {
                cell.DataType = CellValues.Boolean;
                cell.CellValue = new CellValue(Convert.ToBoolean(value));
            }
            else if (type == typeof(DateTime))
            {
                cell.DataType = CellValues.Number;
                cell.StyleIndex = 1;
                cell.CellValue = new CellValue(Convert.ToDateTime(value).ToOADate().ToString());
            }
            else
            {
                cell.DataType = CellValues.String;
                cell.CellValue = new CellValue(Convert.ToString(value) ?? string.Empty);
            }

            return cell;
        }
    }
}
