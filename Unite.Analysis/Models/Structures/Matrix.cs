namespace Unite.Analysis.Models.Structures;

public class Matrix<T>
{
    private record struct CellIndex(int ColumnIndex, int RowIndex);
    
    private readonly string _rootColumnName = "rows/columns";
    private readonly Dictionary<string, int> _columns = [];
    private readonly Dictionary<string, int> _rows = [];
    private readonly Dictionary<CellIndex, T> _values = [];

    public IEnumerable<string> ColumnKeys
    {
        get { return _columns.Keys; }
    }

    public IEnumerable<string> RowKeys
    {
        get { return _rows.Keys; }
    }

    public T this[string columnKey, string rowKey]
    {
        get
        {
            var cellIndex = GetCellIndex(columnKey, rowKey);

            return _values.TryGetValue(cellIndex, out var value) ? value : default;
        }
        set
        {
            var columnIndex = GetOrAddColumnIndex(columnKey);
            var rowIndex = GetOrAddRowIndex(rowKey);
            var cellIndex = new CellIndex(columnIndex, rowIndex);

            _values[cellIndex] = value;
        }
    }

    public bool IsEmpty
    {
        get { return _values.Count == 0; }
    }

    public bool IsNotEmpty
    {
        get { return _values.Count > 0; }
    }


    public Matrix()
    {
    }

    public Matrix(string rootColumnName)
    {
        _rootColumnName = rootColumnName;
    }


    public bool ContainsColumn(string columnKey)
    {
        return _columns.ContainsKey(columnKey);
    }

    public bool ContainsRow(string rowKey)
    {
        return _rows.ContainsKey(rowKey);
    }

    public IReadOnlyDictionary<string, T> GetRow(string key)
    {
        var row = new Dictionary<string, T>();

        var rowIndex = GetRowIndex(key);

        foreach (var column in _columns)
        {
            var columnIndex = column.Value;
            var cellIndex = new CellIndex(columnIndex, rowIndex);

            row[column.Key] = _values.TryGetValue(cellIndex, out var value) ? value : default;
        }

        return row;
    }

    public IReadOnlyDictionary<string, T> GetColumn(string key)
    {
        var column = new Dictionary<string, T>();

        var columnIndex = GetColumnIndex(key);

        foreach (var row in _rows)
        {
            var rowIndex = row.Value;
            var cellIndex = new CellIndex(columnIndex, rowIndex);

            column[row.Key] = _values.TryGetValue(cellIndex, out var value) ? value : default;
        }

        return column;
    }

    public bool TryGet(string columnKey, string rowKey, out T value)
    {
        value = default;

        if (!_columns.TryGetValue(columnKey, out var columnIndex))
            return false;

        if (!_rows.TryGetValue(rowKey, out var rowIndex))
            return false;

        var cellIndex = new CellIndex(columnIndex, rowIndex);

        return _values.TryGetValue(cellIndex, out value);
    }

    public void Remove(string columnKey, string rowKey)
    {
        if (!_columns.TryGetValue(columnKey, out var columnIndex))
            return;

        if (!_rows.TryGetValue(rowKey, out var rowIndex))
            return;

        var cellIndex = new CellIndex(columnIndex, rowIndex);

        _values.Remove(cellIndex);
    }

    public void WriteTo(string path)
    {
        using var stream = new FileStream(path, FileMode.Create, FileAccess.Write);

        WriteTo(stream);
    }

    public void WriteTo(Stream stream)
    {
        using var writer = new StreamWriter(stream);

        var columnKeys = ColumnKeys.ToArray();
        var rowKeys = RowKeys.ToArray();

        writer.Write(_rootColumnName + "\t");

        writer.WriteLine(string.Join("\t", columnKeys));

        foreach (var rowKey in rowKeys)
        {
            writer.Write(rowKey + "\t");

            foreach (var columnKey in columnKeys)
            {
                if (TryGet(columnKey, rowKey, out var value))
                    writer.Write(value + "\t");
                else
                    writer.Write("\t");
            }

            writer.WriteLine();
        }
    }

    public static Matrix<T> ReadFrom(string path)
    {
        using var stream = new FileStream(path, FileMode.Open, FileAccess.Read);

        return ReadFrom(stream);
    }

    public static Matrix<T> ReadFrom(Stream stream)
    {
        using var reader = new StreamReader(stream);

        var headerLine = reader.ReadLine();
        if (string.IsNullOrEmpty(headerLine))
            throw new InvalidDataException("The input stream is empty.");

        var headerParts = headerLine.Split('\t');
        if (headerParts.Length < 2)
            throw new InvalidDataException("The header line must contain at least two columns.");

        var rootColumnName = headerParts[0];
        var columnKeys = headerParts.Skip(1).ToArray();
        var matrix = new Matrix<T>(rootColumnName);

        while (!reader.EndOfStream)
        {
            var line = reader.ReadLine();
            if (line == null)
                break;

            var parts = line.Split('\t');
            if (parts.Length != columnKeys.Length + 1)
                throw new InvalidDataException($"Each row must contain {columnKeys.Length + 1} columns.");

            var rowKey = parts[0];
            for (int i = 0; i < columnKeys.Length; i++)
            {
                var columnKey = columnKeys[i];
                var valueString = parts[i + 1];

                if (TryParseValue(valueString, out var value))
                    matrix[columnKey, rowKey] = value;
            }            
        }

        return matrix;
    }

    public override string ToString()
    {
        using var stream = new MemoryStream();
        
        WriteTo(stream);

        stream.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(stream);

        return reader.ReadToEnd();
    }


    private CellIndex GetCellIndex(string columnKey, string rowKey)
    {
        if (!_columns.TryGetValue(columnKey, out var columnIndex))
            throw new KeyNotFoundException($"The specified column '{columnKey}' does not exist.");

        if (!_rows.TryGetValue(rowKey, out var rowIndex))
            throw new KeyNotFoundException($"The specified row '{rowKey}' does not exist.");

        return new CellIndex(columnIndex, rowIndex);
    }

    private int GetColumnIndex(string columnKey)
    {
        if (_columns.TryGetValue(columnKey, out var columnIndex))
            return columnIndex;

        throw new KeyNotFoundException($"The specified column '{columnKey}' does not exist.");
    }        

    private int GetOrAddColumnIndex(string columnKey)
    {
        if (_columns.TryGetValue(columnKey, out var columnIndex))
            return columnIndex;

        columnIndex = _columns.Count;
        _columns[columnKey] = columnIndex;

        return columnIndex;
    }

    private int GetRowIndex(string rowKey)
    {
        if (_rows.TryGetValue(rowKey, out var rowIndex))
            return rowIndex;

        throw new KeyNotFoundException($"The specified row '{rowKey}' does not exist.");
    }

    private int GetOrAddRowIndex(string rowKey)
    {
        if (_rows.TryGetValue(rowKey, out var rowIndex))
            return rowIndex;

        rowIndex = _rows.Count;
        _rows[rowKey] = rowIndex;

        return rowIndex;
    }

    private static bool TryParseValue(string valueString, out T value)
    {
        value = default;

        if (string.IsNullOrEmpty(valueString))
            return false;

        try
        {
            value = (T)Convert.ChangeType(valueString, typeof(T));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
