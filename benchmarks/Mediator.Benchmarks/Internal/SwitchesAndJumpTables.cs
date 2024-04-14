using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace Mediator.Benchmarks.Internal;

public interface ISwitchMessage
{
    int TypeCode { get; }
}

// csharpier-ignore-start
public sealed record SwitchMessage1() : ISwitchMessage { public int TypeCode => 1; public override string ToString() => "1"; }
public sealed record SwitchMessage2() : ISwitchMessage { public int TypeCode => 2; public override string ToString() => "2"; }
public sealed record SwitchMessage3() : ISwitchMessage { public int TypeCode => 3; public override string ToString() => "3"; }
public sealed record SwitchMessage4() : ISwitchMessage { public int TypeCode => 4; public override string ToString() => "4"; }
public sealed record SwitchMessage5() : ISwitchMessage { public int TypeCode => 5; public override string ToString() => "5"; }
public sealed record SwitchMessage6() : ISwitchMessage { public int TypeCode => 6; public override string ToString() => "6"; }
public sealed record SwitchMessage7() : ISwitchMessage { public int TypeCode => 7; public override string ToString() => "7"; }
public sealed record SwitchMessage8() : ISwitchMessage { public int TypeCode => 8; public override string ToString() => "8"; }
public sealed record SwitchMessage9() : ISwitchMessage { public int TypeCode => 9; public override string ToString() => "9"; }
public sealed record SwitchMessage10() : ISwitchMessage { public int TypeCode => 10; public override string ToString() => "10"; }
public sealed record SwitchMessage11() : ISwitchMessage { public int TypeCode => 11; public override string ToString() => "11"; }
public sealed record SwitchMessage12() : ISwitchMessage { public int TypeCode => 12; public override string ToString() => "12"; }
public sealed record SwitchMessage13() : ISwitchMessage { public int TypeCode => 13; public override string ToString() => "13"; }
public sealed record SwitchMessage14() : ISwitchMessage { public int TypeCode => 14; public override string ToString() => "14"; }
public sealed record SwitchMessage15() : ISwitchMessage { public int TypeCode => 15; public override string ToString() => "15"; }
public sealed record SwitchMessage16() : ISwitchMessage { public int TypeCode => 16; public override string ToString() => "16"; }
public sealed record SwitchMessage17() : ISwitchMessage { public int TypeCode => 17; public override string ToString() => "17"; }
public sealed record SwitchMessage18() : ISwitchMessage { public int TypeCode => 18; public override string ToString() => "18"; }
public sealed record SwitchMessage19() : ISwitchMessage { public int TypeCode => 19; public override string ToString() => "19"; }
public sealed record SwitchMessage20() : ISwitchMessage { public int TypeCode => 20; public override string ToString() => "20"; }
public sealed record SwitchMessage21() : ISwitchMessage { public int TypeCode => 21; public override string ToString() => "21"; }
public sealed record SwitchMessage22() : ISwitchMessage { public int TypeCode => 22; public override string ToString() => "22"; }
public sealed record SwitchMessage23() : ISwitchMessage { public int TypeCode => 23; public override string ToString() => "23"; }
public sealed record SwitchMessage24() : ISwitchMessage { public int TypeCode => 24; public override string ToString() => "24"; }
public sealed record SwitchMessage25() : ISwitchMessage { public int TypeCode => 25; public override string ToString() => "25"; }
public sealed record SwitchMessage26() : ISwitchMessage { public int TypeCode => 26; public override string ToString() => "26"; }
public sealed record SwitchMessage27() : ISwitchMessage { public int TypeCode => 27; public override string ToString() => "27"; }
public sealed record SwitchMessage28() : ISwitchMessage { public int TypeCode => 28; public override string ToString() => "28"; }
public sealed record SwitchMessage29() : ISwitchMessage { public int TypeCode => 29; public override string ToString() => "29"; }
public sealed record SwitchMessage30() : ISwitchMessage { public int TypeCode => 30; public override string ToString() => "30"; }
public sealed record SwitchMessage31() : ISwitchMessage { public int TypeCode => 31; public override string ToString() => "31"; }
public sealed record SwitchMessage32() : ISwitchMessage { public int TypeCode => 32; public override string ToString() => "32"; }
public sealed record SwitchMessage33() : ISwitchMessage { public int TypeCode => 33; public override string ToString() => "33"; }
public sealed record SwitchMessage34() : ISwitchMessage { public int TypeCode => 34; public override string ToString() => "34"; }
public sealed record SwitchMessage35() : ISwitchMessage { public int TypeCode => 35; public override string ToString() => "35"; }
public sealed record SwitchMessage36() : ISwitchMessage { public int TypeCode => 36; public override string ToString() => "36"; }
public sealed record SwitchMessage37() : ISwitchMessage { public int TypeCode => 37; public override string ToString() => "37"; }
public sealed record SwitchMessage38() : ISwitchMessage { public int TypeCode => 38; public override string ToString() => "38"; }
public sealed record SwitchMessage39() : ISwitchMessage { public int TypeCode => 39; public override string ToString() => "39"; }
public sealed record SwitchMessage40() : ISwitchMessage { public int TypeCode => 40; public override string ToString() => "40"; }
public sealed record SwitchMessage41() : ISwitchMessage { public int TypeCode => 41; public override string ToString() => "41"; }
public sealed record SwitchMessage42() : ISwitchMessage { public int TypeCode => 42; public override string ToString() => "42"; }
public sealed record SwitchMessage43() : ISwitchMessage { public int TypeCode => 43; public override string ToString() => "43"; }
public sealed record SwitchMessage44() : ISwitchMessage { public int TypeCode => 44; public override string ToString() => "44"; }
public sealed record SwitchMessage45() : ISwitchMessage { public int TypeCode => 45; public override string ToString() => "45"; }
public sealed record SwitchMessage46() : ISwitchMessage { public int TypeCode => 46; public override string ToString() => "46"; }
public sealed record SwitchMessage47() : ISwitchMessage { public int TypeCode => 47; public override string ToString() => "47"; }
public sealed record SwitchMessage48() : ISwitchMessage { public int TypeCode => 48; public override string ToString() => "48"; }
public sealed record SwitchMessage49() : ISwitchMessage { public int TypeCode => 49; public override string ToString() => "49"; }
public sealed record SwitchMessage50() : ISwitchMessage { public int TypeCode => 50; public override string ToString() => "50"; }
// csharpier-ignore-end

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest, MethodOrderPolicy.Declared)]
// [DisassemblyDiagnoser]
[RankColumn]
public class SwitchesAndJumpTables
{
    // csharpier-ignore-start
    private Dictionary<Type, Func<ISwitchMessage, ValueTask>> _handlers;
    private ConcurrentDictionary<Type, Func<ISwitchMessage, ValueTask>> _handlersConcurrent;

    [GlobalSetup]
    public void Setup()
    {
        System.Diagnostics.Debugger.Launch();
        _handlers = new Dictionary<Type, Func<ISwitchMessage, ValueTask>>();
        _handlersConcurrent = new ConcurrentDictionary<Type, Func<ISwitchMessage, ValueTask>>();

        Add(typeof(SwitchMessage1), (ISwitchMessage m) => Handle1(Unsafe.As<SwitchMessage1>(m)));
        Add(typeof(SwitchMessage2), (ISwitchMessage m) => Handle2(Unsafe.As<SwitchMessage2>(m)));
        Add(typeof(SwitchMessage3), (ISwitchMessage m) => Handle3(Unsafe.As<SwitchMessage3>(m)));
        Add(typeof(SwitchMessage4), (ISwitchMessage m) => Handle4(Unsafe.As<SwitchMessage4>(m)));
        Add(typeof(SwitchMessage5), (ISwitchMessage m) => Handle5(Unsafe.As<SwitchMessage5>(m)));
        Add(typeof(SwitchMessage6), (ISwitchMessage m) => Handle6(Unsafe.As<SwitchMessage6>(m)));
        Add(typeof(SwitchMessage7), (ISwitchMessage m) => Handle7(Unsafe.As<SwitchMessage7>(m)));
        Add(typeof(SwitchMessage8), (ISwitchMessage m) => Handle8(Unsafe.As<SwitchMessage8>(m)));
        Add(typeof(SwitchMessage9), (ISwitchMessage m) => Handle9(Unsafe.As<SwitchMessage9>(m)));
        Add(typeof(SwitchMessage10), (ISwitchMessage m) => Handle10(Unsafe.As<SwitchMessage10>(m)));
        Add(typeof(SwitchMessage11), (ISwitchMessage m) => Handle11(Unsafe.As<SwitchMessage11>(m)));
        Add(typeof(SwitchMessage12), (ISwitchMessage m) => Handle12(Unsafe.As<SwitchMessage12>(m)));
        Add(typeof(SwitchMessage13), (ISwitchMessage m) => Handle13(Unsafe.As<SwitchMessage13>(m)));
        Add(typeof(SwitchMessage14), (ISwitchMessage m) => Handle14(Unsafe.As<SwitchMessage14>(m)));
        Add(typeof(SwitchMessage15), (ISwitchMessage m) => Handle15(Unsafe.As<SwitchMessage15>(m)));
        Add(typeof(SwitchMessage16), (ISwitchMessage m) => Handle16(Unsafe.As<SwitchMessage16>(m)));
        Add(typeof(SwitchMessage17), (ISwitchMessage m) => Handle17(Unsafe.As<SwitchMessage17>(m)));
        Add(typeof(SwitchMessage18), (ISwitchMessage m) => Handle18(Unsafe.As<SwitchMessage18>(m)));
        Add(typeof(SwitchMessage19), (ISwitchMessage m) => Handle19(Unsafe.As<SwitchMessage19>(m)));
        Add(typeof(SwitchMessage20), (ISwitchMessage m) => Handle20(Unsafe.As<SwitchMessage20>(m)));
        Add(typeof(SwitchMessage21), (ISwitchMessage m) => Handle21(Unsafe.As<SwitchMessage21>(m)));
        Add(typeof(SwitchMessage22), (ISwitchMessage m) => Handle22(Unsafe.As<SwitchMessage22>(m)));
        Add(typeof(SwitchMessage23), (ISwitchMessage m) => Handle23(Unsafe.As<SwitchMessage23>(m)));
        Add(typeof(SwitchMessage24), (ISwitchMessage m) => Handle24(Unsafe.As<SwitchMessage24>(m)));
        Add(typeof(SwitchMessage25), (ISwitchMessage m) => Handle25(Unsafe.As<SwitchMessage25>(m)));
        Add(typeof(SwitchMessage26), (ISwitchMessage m) => Handle26(Unsafe.As<SwitchMessage26>(m)));
        Add(typeof(SwitchMessage27), (ISwitchMessage m) => Handle27(Unsafe.As<SwitchMessage27>(m)));
        Add(typeof(SwitchMessage28), (ISwitchMessage m) => Handle28(Unsafe.As<SwitchMessage28>(m)));
        Add(typeof(SwitchMessage29), (ISwitchMessage m) => Handle29(Unsafe.As<SwitchMessage29>(m)));
        Add(typeof(SwitchMessage30), (ISwitchMessage m) => Handle30(Unsafe.As<SwitchMessage30>(m)));
        Add(typeof(SwitchMessage31), (ISwitchMessage m) => Handle31(Unsafe.As<SwitchMessage31>(m)));
        Add(typeof(SwitchMessage32), (ISwitchMessage m) => Handle32(Unsafe.As<SwitchMessage32>(m)));
        Add(typeof(SwitchMessage33), (ISwitchMessage m) => Handle33(Unsafe.As<SwitchMessage33>(m)));
        Add(typeof(SwitchMessage34), (ISwitchMessage m) => Handle34(Unsafe.As<SwitchMessage34>(m)));
        Add(typeof(SwitchMessage35), (ISwitchMessage m) => Handle35(Unsafe.As<SwitchMessage35>(m)));
        Add(typeof(SwitchMessage36), (ISwitchMessage m) => Handle36(Unsafe.As<SwitchMessage36>(m)));
        Add(typeof(SwitchMessage37), (ISwitchMessage m) => Handle37(Unsafe.As<SwitchMessage37>(m)));
        Add(typeof(SwitchMessage38), (ISwitchMessage m) => Handle38(Unsafe.As<SwitchMessage38>(m)));
        Add(typeof(SwitchMessage39), (ISwitchMessage m) => Handle39(Unsafe.As<SwitchMessage39>(m)));
        Add(typeof(SwitchMessage40), (ISwitchMessage m) => Handle40(Unsafe.As<SwitchMessage40>(m)));
        Add(typeof(SwitchMessage41), (ISwitchMessage m) => Handle41(Unsafe.As<SwitchMessage41>(m)));
        Add(typeof(SwitchMessage42), (ISwitchMessage m) => Handle42(Unsafe.As<SwitchMessage42>(m)));
        Add(typeof(SwitchMessage43), (ISwitchMessage m) => Handle43(Unsafe.As<SwitchMessage43>(m)));
        Add(typeof(SwitchMessage44), (ISwitchMessage m) => Handle44(Unsafe.As<SwitchMessage44>(m)));
        Add(typeof(SwitchMessage45), (ISwitchMessage m) => Handle45(Unsafe.As<SwitchMessage45>(m)));
        Add(typeof(SwitchMessage46), (ISwitchMessage m) => Handle46(Unsafe.As<SwitchMessage46>(m)));
        Add(typeof(SwitchMessage47), (ISwitchMessage m) => Handle47(Unsafe.As<SwitchMessage47>(m)));
        Add(typeof(SwitchMessage48), (ISwitchMessage m) => Handle48(Unsafe.As<SwitchMessage48>(m)));
        Add(typeof(SwitchMessage49), (ISwitchMessage m) => Handle49(Unsafe.As<SwitchMessage49>(m)));
        Add(typeof(SwitchMessage50), (ISwitchMessage m) => Handle50(Unsafe.As<SwitchMessage50>(m)));

        void Add(Type type, Func<ISwitchMessage, ValueTask> handler)
        {
            _handlers.Add(type, handler);
            _handlersConcurrent.TryAdd(type, handler);
        }
    }

    public IEnumerable<ISwitchMessage> Messages()
    {
        yield return new SwitchMessage45();
    }

    [Benchmark]
    [ArgumentsSource(nameof(Messages))]
    public ValueTask SwitchType(ISwitchMessage message)
    {
        switch (message)
        {
            case SwitchMessage1 m1: return Handle1(m1);
            case SwitchMessage2 m2: return Handle2(m2);
            case SwitchMessage3 m3: return Handle3(m3);
            case SwitchMessage4 m4: return Handle4(m4);
            case SwitchMessage5 m5: return Handle5(m5);
            case SwitchMessage6 m6: return Handle6(m6);
            case SwitchMessage7 m7: return Handle7(m7);
            case SwitchMessage8 m8: return Handle8(m8);
            case SwitchMessage9 m9: return Handle9(m9);
            case SwitchMessage10 m10: return Handle10(m10);
            case SwitchMessage11 m11: return Handle11(m11);
            case SwitchMessage12 m12: return Handle12(m12);
            case SwitchMessage13 m13: return Handle13(m13);
            case SwitchMessage14 m14: return Handle14(m14);
            case SwitchMessage15 m15: return Handle15(m15);
            case SwitchMessage16 m16: return Handle16(m16);
            case SwitchMessage17 m17: return Handle17(m17);
            case SwitchMessage18 m18: return Handle18(m18);
            case SwitchMessage19 m19: return Handle19(m19);
            case SwitchMessage20 m20: return Handle20(m20);
            case SwitchMessage21 m21: return Handle21(m21);
            case SwitchMessage22 m22: return Handle22(m22);
            case SwitchMessage23 m23: return Handle23(m23);
            case SwitchMessage24 m24: return Handle24(m24);
            case SwitchMessage25 m25: return Handle25(m25);
            case SwitchMessage26 m26: return Handle26(m26);
            case SwitchMessage27 m27: return Handle27(m27);
            case SwitchMessage28 m28: return Handle28(m28);
            case SwitchMessage29 m29: return Handle29(m29);
            case SwitchMessage30 m30: return Handle30(m30);
            case SwitchMessage31 m31: return Handle31(m31);
            case SwitchMessage32 m32: return Handle32(m32);
            case SwitchMessage33 m33: return Handle33(m33);
            case SwitchMessage34 m34: return Handle34(m34);
            case SwitchMessage35 m35: return Handle35(m35);
            case SwitchMessage36 m36: return Handle36(m36);
            case SwitchMessage37 m37: return Handle37(m37);
            case SwitchMessage38 m38: return Handle38(m38);
            case SwitchMessage39 m39: return Handle39(m39);
            case SwitchMessage40 m40: return Handle40(m40);
            case SwitchMessage41 m41: return Handle41(m41);
            case SwitchMessage42 m42: return Handle42(m42);
            case SwitchMessage43 m43: return Handle43(m43);
            case SwitchMessage44 m44: return Handle44(m44);
            case SwitchMessage45 m45: return Handle45(m45);
            case SwitchMessage46 m46: return Handle46(m46);
            case SwitchMessage47 m47: return Handle47(m47);
            case SwitchMessage48 m48: return Handle48(m48);
            case SwitchMessage49 m49: return Handle49(m49);
            case SwitchMessage50 m50: return Handle50(m50);
            default: return default;
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Messages))]
    public ValueTask SwitchGetType(ISwitchMessage message)
    {
        if (message.GetType() == typeof(SwitchMessage1)) return Handle1(Unsafe.As<SwitchMessage1>(message));
        else if (message.GetType() == typeof(SwitchMessage2)) return Handle2(Unsafe.As<SwitchMessage2>(message));
        else if (message.GetType() == typeof(SwitchMessage3)) return Handle3(Unsafe.As<SwitchMessage3>(message));
        else if (message.GetType() == typeof(SwitchMessage4)) return Handle4(Unsafe.As<SwitchMessage4>(message));
        else if (message.GetType() == typeof(SwitchMessage5)) return Handle5(Unsafe.As<SwitchMessage5>(message));
        else if (message.GetType() == typeof(SwitchMessage6)) return Handle6(Unsafe.As<SwitchMessage6>(message));
        else if (message.GetType() == typeof(SwitchMessage7)) return Handle7(Unsafe.As<SwitchMessage7>(message));
        else if (message.GetType() == typeof(SwitchMessage8)) return Handle8(Unsafe.As<SwitchMessage8>(message));
        else if (message.GetType() == typeof(SwitchMessage9)) return Handle9(Unsafe.As<SwitchMessage9>(message));
        else if (message.GetType() == typeof(SwitchMessage10)) return Handle10(Unsafe.As<SwitchMessage10>(message));
        else if (message.GetType() == typeof(SwitchMessage11)) return Handle11(Unsafe.As<SwitchMessage11>(message));
        else if (message.GetType() == typeof(SwitchMessage12)) return Handle12(Unsafe.As<SwitchMessage12>(message));
        else if (message.GetType() == typeof(SwitchMessage13)) return Handle13(Unsafe.As<SwitchMessage13>(message));
        else if (message.GetType() == typeof(SwitchMessage14)) return Handle14(Unsafe.As<SwitchMessage14>(message));
        else if (message.GetType() == typeof(SwitchMessage15)) return Handle15(Unsafe.As<SwitchMessage15>(message));
        else if (message.GetType() == typeof(SwitchMessage16)) return Handle16(Unsafe.As<SwitchMessage16>(message));
        else if (message.GetType() == typeof(SwitchMessage17)) return Handle17(Unsafe.As<SwitchMessage17>(message));
        else if (message.GetType() == typeof(SwitchMessage18)) return Handle18(Unsafe.As<SwitchMessage18>(message));
        else if (message.GetType() == typeof(SwitchMessage19)) return Handle19(Unsafe.As<SwitchMessage19>(message));
        else if (message.GetType() == typeof(SwitchMessage20)) return Handle20(Unsafe.As<SwitchMessage20>(message));
        else if (message.GetType() == typeof(SwitchMessage21)) return Handle21(Unsafe.As<SwitchMessage21>(message));
        else if (message.GetType() == typeof(SwitchMessage22)) return Handle22(Unsafe.As<SwitchMessage22>(message));
        else if (message.GetType() == typeof(SwitchMessage23)) return Handle23(Unsafe.As<SwitchMessage23>(message));
        else if (message.GetType() == typeof(SwitchMessage24)) return Handle24(Unsafe.As<SwitchMessage24>(message));
        else if (message.GetType() == typeof(SwitchMessage25)) return Handle25(Unsafe.As<SwitchMessage25>(message));
        else if (message.GetType() == typeof(SwitchMessage26)) return Handle26(Unsafe.As<SwitchMessage26>(message));
        else if (message.GetType() == typeof(SwitchMessage27)) return Handle27(Unsafe.As<SwitchMessage27>(message));
        else if (message.GetType() == typeof(SwitchMessage28)) return Handle28(Unsafe.As<SwitchMessage28>(message));
        else if (message.GetType() == typeof(SwitchMessage29)) return Handle29(Unsafe.As<SwitchMessage29>(message));
        else if (message.GetType() == typeof(SwitchMessage30)) return Handle30(Unsafe.As<SwitchMessage30>(message));
        else if (message.GetType() == typeof(SwitchMessage31)) return Handle31(Unsafe.As<SwitchMessage31>(message));
        else if (message.GetType() == typeof(SwitchMessage32)) return Handle32(Unsafe.As<SwitchMessage32>(message));
        else if (message.GetType() == typeof(SwitchMessage33)) return Handle33(Unsafe.As<SwitchMessage33>(message));
        else if (message.GetType() == typeof(SwitchMessage34)) return Handle34(Unsafe.As<SwitchMessage34>(message));
        else if (message.GetType() == typeof(SwitchMessage35)) return Handle35(Unsafe.As<SwitchMessage35>(message));
        else if (message.GetType() == typeof(SwitchMessage36)) return Handle36(Unsafe.As<SwitchMessage36>(message));
        else if (message.GetType() == typeof(SwitchMessage37)) return Handle37(Unsafe.As<SwitchMessage37>(message));
        else if (message.GetType() == typeof(SwitchMessage38)) return Handle38(Unsafe.As<SwitchMessage38>(message));
        else if (message.GetType() == typeof(SwitchMessage39)) return Handle39(Unsafe.As<SwitchMessage39>(message));
        else if (message.GetType() == typeof(SwitchMessage40)) return Handle40(Unsafe.As<SwitchMessage40>(message));
        else if (message.GetType() == typeof(SwitchMessage41)) return Handle41(Unsafe.As<SwitchMessage41>(message));
        else if (message.GetType() == typeof(SwitchMessage42)) return Handle42(Unsafe.As<SwitchMessage42>(message));
        else if (message.GetType() == typeof(SwitchMessage43)) return Handle43(Unsafe.As<SwitchMessage43>(message));
        else if (message.GetType() == typeof(SwitchMessage44)) return Handle44(Unsafe.As<SwitchMessage44>(message));
        else if (message.GetType() == typeof(SwitchMessage45)) return Handle45(Unsafe.As<SwitchMessage45>(message));
        else if (message.GetType() == typeof(SwitchMessage46)) return Handle46(Unsafe.As<SwitchMessage46>(message));
        else if (message.GetType() == typeof(SwitchMessage47)) return Handle47(Unsafe.As<SwitchMessage47>(message));
        else if (message.GetType() == typeof(SwitchMessage48)) return Handle48(Unsafe.As<SwitchMessage48>(message));
        else if (message.GetType() == typeof(SwitchMessage49)) return Handle49(Unsafe.As<SwitchMessage49>(message));
        else if (message.GetType() == typeof(SwitchMessage50)) return Handle50(Unsafe.As<SwitchMessage50>(message));
        return default;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Messages))]
    public ValueTask SwitchCode(ISwitchMessage message)
    {
        switch (message.TypeCode)
        {
            case 1: return Handle1(Unsafe.As<SwitchMessage1>(message));
            case 2: return Handle2(Unsafe.As<SwitchMessage2>(message));
            case 3: return Handle3(Unsafe.As<SwitchMessage3>(message));
            case 4: return Handle4(Unsafe.As<SwitchMessage4>(message));
            case 5: return Handle5(Unsafe.As<SwitchMessage5>(message));
            case 6: return Handle6(Unsafe.As<SwitchMessage6>(message));
            case 7: return Handle7(Unsafe.As<SwitchMessage7>(message));
            case 8: return Handle8(Unsafe.As<SwitchMessage8>(message));
            case 9: return Handle9(Unsafe.As<SwitchMessage9>(message));
            case 10: return Handle10(Unsafe.As<SwitchMessage10>(message));
            case 11: return Handle11(Unsafe.As<SwitchMessage11>(message));
            case 12: return Handle12(Unsafe.As<SwitchMessage12>(message));
            case 13: return Handle13(Unsafe.As<SwitchMessage13>(message));
            case 14: return Handle14(Unsafe.As<SwitchMessage14>(message));
            case 15: return Handle15(Unsafe.As<SwitchMessage15>(message));
            case 16: return Handle16(Unsafe.As<SwitchMessage16>(message));
            case 17: return Handle17(Unsafe.As<SwitchMessage17>(message));
            case 18: return Handle18(Unsafe.As<SwitchMessage18>(message));
            case 19: return Handle19(Unsafe.As<SwitchMessage19>(message));
            case 20: return Handle20(Unsafe.As<SwitchMessage20>(message));
            case 21: return Handle21(Unsafe.As<SwitchMessage21>(message));
            case 22: return Handle22(Unsafe.As<SwitchMessage22>(message));
            case 23: return Handle23(Unsafe.As<SwitchMessage23>(message));
            case 24: return Handle24(Unsafe.As<SwitchMessage24>(message));
            case 25: return Handle25(Unsafe.As<SwitchMessage25>(message));
            case 26: return Handle26(Unsafe.As<SwitchMessage26>(message));
            case 27: return Handle27(Unsafe.As<SwitchMessage27>(message));
            case 28: return Handle28(Unsafe.As<SwitchMessage28>(message));
            case 29: return Handle29(Unsafe.As<SwitchMessage29>(message));
            case 30: return Handle30(Unsafe.As<SwitchMessage30>(message));
            case 31: return Handle31(Unsafe.As<SwitchMessage31>(message));
            case 32: return Handle32(Unsafe.As<SwitchMessage32>(message));
            case 33: return Handle33(Unsafe.As<SwitchMessage33>(message));
            case 34: return Handle34(Unsafe.As<SwitchMessage34>(message));
            case 35: return Handle35(Unsafe.As<SwitchMessage35>(message));
            case 36: return Handle36(Unsafe.As<SwitchMessage36>(message));
            case 37: return Handle37(Unsafe.As<SwitchMessage37>(message));
            case 38: return Handle38(Unsafe.As<SwitchMessage38>(message));
            case 39: return Handle39(Unsafe.As<SwitchMessage39>(message));
            case 40: return Handle40(Unsafe.As<SwitchMessage40>(message));
            case 41: return Handle41(Unsafe.As<SwitchMessage41>(message));
            case 42: return Handle42(Unsafe.As<SwitchMessage42>(message));
            case 43: return Handle43(Unsafe.As<SwitchMessage43>(message));
            case 44: return Handle44(Unsafe.As<SwitchMessage44>(message));
            case 45: return Handle45(Unsafe.As<SwitchMessage45>(message));
            case 46: return Handle46(Unsafe.As<SwitchMessage46>(message));
            case 47: return Handle47(Unsafe.As<SwitchMessage47>(message));
            case 48: return Handle48(Unsafe.As<SwitchMessage48>(message));
            case 49: return Handle49(Unsafe.As<SwitchMessage49>(message));
            case 50: return Handle50(Unsafe.As<SwitchMessage50>(message));
            default: return default;
        }
    }

    [Benchmark]
    [ArgumentsSource(nameof(Messages))]
    public ValueTask DictionaryLookup(ISwitchMessage message) => _handlers[message.GetType()](message);

    [Benchmark]
    [ArgumentsSource(nameof(Messages))]
    public ValueTask ConcurrentDictionaryLookup(ISwitchMessage message) => _handlersConcurrent[message.GetType()](message);

    private ValueTask Handle1(SwitchMessage1 _) => default;
    private ValueTask Handle2(SwitchMessage2 _) => default;
    private ValueTask Handle3(SwitchMessage3 _) => default;
    private ValueTask Handle4(SwitchMessage4 _) => default;
    private ValueTask Handle5(SwitchMessage5 _) => default;
    private ValueTask Handle6(SwitchMessage6 _) => default;
    private ValueTask Handle7(SwitchMessage7 _) => default;
    private ValueTask Handle8(SwitchMessage8 _) => default;
    private ValueTask Handle9(SwitchMessage9 _) => default;
    private ValueTask Handle10(SwitchMessage10 _) => default;
    private ValueTask Handle11(SwitchMessage11 _) => default;
    private ValueTask Handle12(SwitchMessage12 _) => default;
    private ValueTask Handle13(SwitchMessage13 _) => default;
    private ValueTask Handle14(SwitchMessage14 _) => default;
    private ValueTask Handle15(SwitchMessage15 _) => default;
    private ValueTask Handle16(SwitchMessage16 _) => default;
    private ValueTask Handle17(SwitchMessage17 _) => default;
    private ValueTask Handle18(SwitchMessage18 _) => default;
    private ValueTask Handle19(SwitchMessage19 _) => default;
    private ValueTask Handle20(SwitchMessage20 _) => default;
    private ValueTask Handle21(SwitchMessage21 _) => default;
    private ValueTask Handle22(SwitchMessage22 _) => default;
    private ValueTask Handle23(SwitchMessage23 _) => default;
    private ValueTask Handle24(SwitchMessage24 _) => default;
    private ValueTask Handle25(SwitchMessage25 _) => default;
    private ValueTask Handle26(SwitchMessage26 _) => default;
    private ValueTask Handle27(SwitchMessage27 _) => default;
    private ValueTask Handle28(SwitchMessage28 _) => default;
    private ValueTask Handle29(SwitchMessage29 _) => default;
    private ValueTask Handle30(SwitchMessage30 _) => default;
    private ValueTask Handle31(SwitchMessage31 _) => default;
    private ValueTask Handle32(SwitchMessage32 _) => default;
    private ValueTask Handle33(SwitchMessage33 _) => default;
    private ValueTask Handle34(SwitchMessage34 _) => default;
    private ValueTask Handle35(SwitchMessage35 _) => default;
    private ValueTask Handle36(SwitchMessage36 _) => default;
    private ValueTask Handle37(SwitchMessage37 _) => default;
    private ValueTask Handle38(SwitchMessage38 _) => default;
    private ValueTask Handle39(SwitchMessage39 _) => default;
    private ValueTask Handle40(SwitchMessage40 _) => default;
    private ValueTask Handle41(SwitchMessage41 _) => default;
    private ValueTask Handle42(SwitchMessage42 _) => default;
    private ValueTask Handle43(SwitchMessage43 _) => default;
    private ValueTask Handle44(SwitchMessage44 _) => default;
    private ValueTask Handle45(SwitchMessage45 _) => default;
    private ValueTask Handle46(SwitchMessage46 _) => default;
    private ValueTask Handle47(SwitchMessage47 _) => default;
    private ValueTask Handle48(SwitchMessage48 _) => default;
    private ValueTask Handle49(SwitchMessage49 _) => default;
    private ValueTask Handle50(SwitchMessage50 _) => default;
    // csharpier-ignore-end
}
