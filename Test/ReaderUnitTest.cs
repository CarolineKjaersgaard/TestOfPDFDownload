using System;
using System.Data;
using PDFDownload;

namespace Test;

// Tests that the Readers ReadFile method is able to read from an excel file and return a datatable containing content from
// that file. If there is something wrong with the file, it should return an empty datatable.
public class ReaderUnitTest
{
    // Calls read file method
    // Param: string file = name of file to be called
    // Returns: datatable containing data from file
    private DataTable CallReadFile(string file)
    {
        Reader reader = new Reader();
        string path = Path.Combine(AppContext.BaseDirectory, "TestListFolder", file);
        return reader.ReadFile(path);
    }
    
    // Tests if read file method returns a correctly sized datatable given valid excel file as input
    [Fact]
    public void ReadFile_ValidExcelFile_ReturnsCorrectSizeDataTable()
    {
        DataTable result = CallReadFile("Valid.xlsx");
        Assert.Equal(11, result.Rows.Count); // Valid.xlsx contains 11 rows
    }

    // Tests if read file method returns datatable with correct content given valid excel file as input
    [Fact]
    public void ReadFile_ValidExcelFile_ReturnsCorrectContentDataTable()
    {
        DataTable result = CallReadFile("Valid.xlsx");
        Assert.Equal("BR50041", result.Rows[1][0]); // Content of this cell in Valid.xlsx
    }

    // Tests if read file method returns an empty datatable given an empty excel file as input
    [Fact]
    public void ReadFile_EmptyExcelFile_ReturnsEmptyDataTable()
    {
        DataTable result = CallReadFile("Empty.xlsx");
        Assert.Equal(0, result.Rows.Count);
    }

    // Tests if read file method returns an empty datatable given a file path pointing to a file that does not exist
    [Fact]
    public void ReadFile_FileDoesNotExist_ReturnsEmptyDataTable()
    {
        DataTable result = CallReadFile("NotHere.xlsx");
        Assert.Equal(0, result.Rows.Count);
    }

    // Tests if read file method returns an empty datatable given a file of wrong type
    [Fact]
    public void ReadFile_InvalidFileFormat_ReturnsEmptyDataTable()
    {
        DataTable result = CallReadFile("NotExcel.txt");
        Assert.Equal(0, result.Rows.Count);
    }
}
