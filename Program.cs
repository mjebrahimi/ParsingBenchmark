using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using System.Runtime.CompilerServices;

#if DEBUG
Console.WriteLine("Run benchmark in release mode without debugger attached");
return;
#endif

BenchmarkRunner.Run<Benchmark>();

Console.ReadLine();

[SimpleJob(RunStrategy.Throughput)]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
public class Benchmark
{
    //Example valid inputs
    private static readonly string[] inputs =
    [
        "U",
        "UDU",
        "LUDUDUR",
        "LUR",
        "LUDUR",
        "UDLLURR",
        "LUDLURDUR",
        "UDLURDLLURR",
        "LUDLURRDLURDU",
        "LLUDUDURDUDLURR",
        "LUDUDUDUDLUDURDUR",
        "LLLLUDLURRDURRDUR",
        "LUDUDLLURRDLLLURRRR",
        "LUDLURDLUDLURRDUDUR",
        "UDLUDURDLLLURDURDUR",
        "LLLUDLURRDUDUDURRDU",
        "LLLUDURRDLURDLUDURDUDUR",
        "LUDLUDURDUDUDLURDLURRDU",
        "LUDLLUDLURRDLUDLUDURRRR",
        "LLLLURRRRDLLUDUDURRDUDLURDU",
        "UDUDUDLLLLURDUDUDURRDLURDUR",
        "UDLURDLLLUDURDLLURDURRDUDLLURRR",
        "LLLLUDLUDUDURDLUDLUDURDUDUDURRRRDUR",
        "LLUDLLUDURDURDUDURDUDUDLLLURRDURDLURDUR",
        "LUDUDUDURDLLLUDLURDLLUDURDLURDLUDURRRRRDLUDUDUR",
    ];

    [Benchmark]
    public void Original()
    {
        foreach (var input in inputs)
        {
            OriginalParser.IsMagicPath(input, 0, input.Length - 1);
        }
    }

    [Benchmark]
    public void Improved()
    {
        foreach (var input in inputs)
        {
            Parser.IsMagicPath(input);
        }
    }

    [Benchmark]
    public void ImprovedV2()
    {
        foreach (var input in inputs)
        {
            NewParser.IsMagicPath(input);
        }
    }
}

public class OriginalParser
{
    public static bool IsMagicPath(string _path, int start, int end)
    {
        if (start > end)
        {
            return false;
        }
        else if (IsDetour(_path, start, end))
        {
            return true;
        }
        else
        {
            for (int i = start; i <= end; i++)
            {
                if (_path[i] == 'D' && IsDetour(_path, start, i - 1) && IsMagicPath(_path, i + 1, end))
                {
                    return true;
                }
            }
        }
        return false;
    }

    private static bool IsDetour(string _path, int start, int end)
    {
        if (start == end && _path[start] == 'U')
        {
            return true;
        }
        else if (end - start > 2 && _path[start] == 'L' && _path[end] == 'R')
        {
            return IsMagicPath(_path, start + 1, end - 1);
        }

        return false;
    }
}

public static class Parser
{
    private static string _text;

    public static bool IsMagicPath(string text)
    {
        _text = text;
        return MagicPath(0) >= 0;
    }

    private static int MagicPath(int position)
    {
        switch (_text[position])
        {
            case 'L' or 'U':
                position = Detour(position);
                var newPosition = Consume(position, 'D');
                return newPosition >= 0 ? MagicPath(newPosition) : position;
            default:
                return -1;
        }
    }

    private static int Detour(int position)
    {
        switch (_text[position])
        {
            case 'L':
                position = Consume(position, 'L');
                position = MagicPath(position);
                position = Consume(position, 'R');
                return position;
            case 'U':
                return Consume(position, 'U');
            default:
                return -1;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static int Consume(int position, char expected)
    {
        return position < _text.Length && _text[position] == expected ? position + 1 : -1;
    }
}

public static class NewParser
{
    public static bool IsMagicPath(string text)
    {
        return new ParserStruct().IsMagicPathInternal(text);
    }

    private ref struct ParserStruct
    {
        private ReadOnlySpan<char> _text;

        public bool IsMagicPathInternal(string text)
        {
            _text = text.AsSpan();
            return MagicPath(0) >= 0;
        }

        private int MagicPath(int position)
        {
            switch (_text[position])
            {
                case 'L' or 'U':
                    position = Detour(position);
                    var newPosition = Consume(position, 'D');
                    return newPosition >= 0 ? MagicPath(newPosition) : position;
                default:
                    return -1;
            }
        }

        private int Detour(int position)
        {
            switch (_text[position])
            {
                case 'L':
                    position = Consume(position, 'L');
                    position = MagicPath(position);
                    position = Consume(position, 'R');
                    return position;
                case 'U':
                    return Consume(position, 'U');
                default:
                    return -1;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly int Consume(int position, char expected)
        {
            return position < _text.Length && _text[position] == expected ? position + 1 : -1;
        }
    }
}