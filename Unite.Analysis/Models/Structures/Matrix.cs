namespace Unite.Analysis.Models.Structures;

public class Matrix<T>
{
    private readonly string _rootColumnName = "rows/columns";
    private readonly Dictionary<string, Dictionary<string, T>> _columns = [];
    private readonly Dictionary<string, Dictionary<string, T>> _rows = [];

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
            return _columns[columnKey][rowKey];
        }
        set
        {
            if (!_columns.ContainsKey(columnKey))
                _columns[columnKey] = [];

            if (!_rows.ContainsKey(rowKey))
                _rows[rowKey] = [];

            _columns[columnKey][rowKey] = value;
            _rows[rowKey][columnKey] = value;
        }
    }

    public bool IsEmpty
    {
        get { return _rows.Count == 0; }
    }

    public bool IsNotEmpty
    {
        get { return !IsEmpty; }
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
        return _rows.TryGetValue(key, out var columns) ? columns : [];
    }

    public IReadOnlyDictionary<string, T> GetColumn(string key)
    {
        return _columns.TryGetValue(key, out var rows) ? rows : [];
    }

    public bool TryGet(string columnKey, string rowKey, out T value)
    {
        value = default;

        return _columns.TryGetValue(columnKey, out var column) && column.TryGetValue(rowKey, out value);
    }

    public void Remove(string columnKey, string rowKey)
    {
        _columns.TryGetValue(columnKey, out var column);
        column?.Remove(rowKey);

        _rows.TryGetValue(rowKey, out var row);
        row?.Remove(columnKey);
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
