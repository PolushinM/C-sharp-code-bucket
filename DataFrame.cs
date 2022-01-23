public class DataFrame {
 
  // Headers of columns
  private List<string> columns_;
 
  public List<string> Columns {
    get => columns_;
    set {
      columns_ = value;
    }
  }
 
  // Data storage
  private List<List<decimal>> data_;
 
  public int Count {
    get => data_.Count;
  }
 
  // Constructors
  public DataFrame(int size = 0) {
    columns_ = new List<string>();
    data_ = new List<List<decimal>>(size + 1);
  }
 
  public DataFrame(List<string> columns, int size = 0) {
    columns_ = new List<string>(columns);
    data_ = new List<List<decimal>>(size + 1);
  }
 
  // Row manage methods
  public bool AddRow(params decimal[] row) {
    if (row.Length == columns_.Count) {
      List<decimal> newRow = new List<decimal>(row.Length);
      newRow.AddRange(row);
      data_.Add(newRow);
      return true;
    }
    return false;
  }
 
  public bool AddRow(List<decimal> row) {
    if (row.Count == columns_.Count) {
      data_.Add(row);
      return true;
    }
    return false;
  }
 
  public bool DropRow(int index) {
    if ((index < data_.Count) && (index > 0)) {
      data_.RemoveAt(index);
      return true;
    }
    return false;
  }
 
  public bool UpdateRow(int index, params decimal[] row) {
    if (row.Length == columns_.Count) {
      List<decimal> newRow = new List<decimal>(row.Length);
      newRow.AddRange(row);
      data_[index] = newRow;
      return true;
    }
    return false;
  }
 
  public bool UpdateRow(int index, List<decimal> row) {
    if (row.Count == columns_.Count) {
      data_[index] = row;
      return true;
    }
    return false;
  }
 
  // Read/Write files
  public async void ToCSV(string path, char sep = ',') {
    try {
      using (StreamWriter sw = new StreamWriter(path, false, System.Text.Encoding.Default)) {
        // Write columns
        await sw.WriteLineAsync(String.Join(sep.ToString(), columns_));
        // Writw data
        foreach (List<decimal> row in data_) {
          // List for converted to string values
          List<string> strRow = new List<string>();
          // Convert to string
          foreach (decimal value in row) {
            strRow.Add(value.ToString());
          }
          // Write new row
          await sw.WriteLineAsync(String.Join(sep.ToString(), strRow));
        }
      }
    }
    catch (Exception e) {
      MessageBox.Show(e.Message);
    }
  }
 
  public void Clear() {
    columns_.Clear();
    data_.Clear();
  }
 
  public void ReadCSV(string path, char sep = ',') {
    // Clear all dataframe
    Clear();
    // Read new columns and data
    try {
      using (StreamReader sr = new StreamReader(path, System.Text.Encoding.Default)) {
        string strRow;    // Raw string row from file
        string[] sepRow;  // Splitted row array
        // Fill columns
        strRow = sr.ReadLine();
        sepRow = strRow.Split(sep);
        columns_.AddRange(sepRow);
        // Fill data
        while ((strRow = sr.ReadLine()) != null) {
          sepRow = strRow.Split(sep);
          List<decimal> row = new List<decimal>(); // Row, ready to add
          for (int i = 0; i < sepRow.Length; ++i) {
            row.Add(Convert.ToDecimal(sepRow[i]));
          }
          data_.Add(row);
        }
      }
    }
    catch (Exception e) {
      MessageBox.Show(e.Message);
    }
  }
 
  // Indexers
  public decimal this[int row, string col] {
    get => data_[row][columns_.FindIndex(value => (value == col))];
    set {
      List<decimal> rowList = data_[row];
      rowList[columns_.FindIndex(value => (value == col))] = value;
      data_[row] = rowList;
    }
  }
 
  public List<decimal> this[int row] {
    get => data_[row];
    set {
      data_[row] = value;
    }
  }
 
}
 
public class DataFrameTester {
 
  public bool Test() {
    bool result = true;
 
    result = CSV() ? result : false;
    result = Indexers() ? result : false;
    result = Methods() ? result : false;
 
    return result;
  }
 
  public bool CSV() {
    bool result = true;
    List<string> columns = new List<string> { "one", "two", "three", "four" };
    DataFrame df = new DataFrame(columns);
    df.AddRow(1.1m, 1.2m, 1.3m, 1.4m);
    df.AddRow(2.1m, 2.2m, 2.3m, 2.4m);
    df.AddRow(3.1m, 3.2m, 3.3m, 3.4m);
    df.AddRow(4.1m, 4.2m, 4.3m, 4.4m);
 
    df.ToCSV("test_df.csv");
 
    DataFrame df1 = new DataFrame();
 
    df1.ReadCSV("test_df.csv", ',');
 
    result = df1[0, "one"] != 1.1m ? false : result;
    result = df1[2, "three"] != 3.3m ? false : result;
    result = df1[3, "four"] != 4.4m ? false : result;
 
    return result;
  }
 
  public bool Indexers() {
    bool result = true;
    DataFrame df = new DataFrame(new List<string> { "zero", "one", "two" });
 
    df.AddRow(0.0m, 0.0m, 0.0m);
    df.AddRow(0.0m, 0.0m, 0.0m);
    df.AddRow(0.0m, 0.0m, 0.0m);
 
    df[0, "one"] = 0.1m;
    df[0, "two"] = 0.2m;
    df[1] = new List<decimal> { 1.0m, 1.1m, 1.2m };
    df[2] = new List<decimal> { 2.0m, 0.0m, 0.0m };
    df[2, "one"] = 2.1m;
    df[2, "two"] = 2.2m;
 
    result = df[0, "zero"] != 0.0m ? false : result;
    result = df[0, "one"] != 0.1m ? false : result;
    result = df[0, "two"] != 0.2m ? false : result;
    result = df[1, "zero"] != 1.0m ? false : result;
    result = df[1][1] != 1.1m ? false : result;
    result = df[1][2] != 1.2m ? false : result;
    result = df[2, "zero"] != 2.0m ? false : result;
    result = df[2, "one"] != 2.1m ? false : result;
    result = df[2][2] != 2.2m ? false : result;
    result = df.Columns[2] != "two" ? false : result;
 
    df.Columns = new List<string> { "zero", "one", "two2" };
    result = df.Columns[2] != "two2" ? false : result;
 
    return result;
  }
 
  public bool Methods() {
    bool result = true;
    DataFrame df = new DataFrame(1000);
 
    df.Columns = new List<string> { "zero", "one", "two" };
 
    List<decimal> zeroList = new List<decimal> { 0.0m, 0.1m, 0.2m };
    List<decimal> oneList = new List<decimal> { 1.0m, 1.1m, 1.2m };
 
    df.AddRow(zeroList);
    df.AddRow(oneList);
    df.AddRow(oneList);
    df.AddRow(oneList);
    df.AddRow(2.0m, 2.1m, 2.2m);
    df.AddRow(2.0m, 2.1m, 2.2m);
    df.AddRow(2.0m, 2.1m, 2.2m);
    df.DropRow(3);
    df.DropRow(1);
 
    df.UpdateRow(3, new List<decimal> { 3.0m, 3.1m, 3.2m });
    df.UpdateRow(4, 4.0m, 4.1m, 4.2m);
 
    result = df[0, "zero"] != 0.0m ? false : result;
    result = df[0, "two"] != 0.2m ? false : result;
    result = df[1, "zero"] != 1.0m ? false : result;
    result = df[1][1] != 1.1m ? false : result;
    result = df[2, "zero"] != 2.0m ? false : result;
    result = df[2, "one"] != 2.1m ? false : result;
    result = df[2][2] != 2.2m ? false : result;
    result = df.Columns[1] != "one" ? false : result;
    result = df[3, "zero"] != 3.0m ? false : result;
    result = df[3, "one"] != 3.1m ? false : result;
    result = df[3, "two"] != 3.2m ? false : result;
    result = df[4, "zero"] != 4.0m ? false : result;
    result = df[4, "one"] != 4.1m ? false : result;
    result = df[4, "two"] != 4.2m ? false : result;
 
    df.Clear();
 
    result = df.Count != 0 ? false : result;
 
    return result;
  }
 
}