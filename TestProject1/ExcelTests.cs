using System.Data;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace TestProject1;

public class ExcelTests {
	public static DataTable ReadExcelSheet(string fname, bool firstRowIsHeader = true) {
		var Headers = new List<string>();
		var dt = new DataTable();
		using (var doc = SpreadsheetDocument.Open(fname, false)) {
			//Read the first Sheets 
			var sheet = doc.WorkbookPart.Workbook.Sheets.GetFirstChild<Sheet>();
			var worksheet = (doc.WorkbookPart.GetPartById(sheet.Id.Value) as WorksheetPart).Worksheet;
			var rows = worksheet.GetFirstChild<SheetData>().Descendants<Row>();
			var counter = 0;
			foreach (var row in rows) {
				counter = counter + 1;
				//Read the first row as header
				if (counter == 1) {
					var j = 1;
					foreach (var cell in row.Descendants<Cell>()) {
						var colunmName = firstRowIsHeader ? GetCellValue(doc, cell) : "Field" + j++;
						Console.WriteLine(colunmName);
						Headers.Add(colunmName);
						dt.Columns.Add(colunmName);
					}
				} else {
					dt.Rows.Add();
					var i = 0;
					foreach (var cell in row.Descendants<Cell>()) {
						dt.Rows[dt.Rows.Count - 1][i] = GetCellValue(doc, cell);
						i++;
					}
				}
			}
		}

		return dt;
	}

	private static string GetCellValue(SpreadsheetDocument doc, Cell cell) {
		var value = cell.CellValue.InnerText;
		if (cell.DataType != null && cell.DataType.Value == CellValues.SharedString) {
			return doc.WorkbookPart.SharedStringTablePart.SharedStringTable.ChildElements[int.Parse(value)]
				.InnerText;
		}

		return value;
	}

	[SetUp]
	public void Setup() {
	}

	[Test]
	public void TestExcelReading() {
		var a = @"../NewFrontier/resources/TestResource.xlsx";
		var path = Path.GetFullPath(Path.Combine(Environment.CurrentDirectory.Split("\\bin\\")[0], a));
		var z = ReadExcelSheet(path);
		foreach (var row in z.AsEnumerable()) {
			for (var i = 0; i < z.Columns.Count; i++) {
				Console.Write($"{z.Columns[i]}: {row.ItemArray[i]} ");
			}

			Console.WriteLine();
		}

		Assert.That(true, Is.False);
	}
}
