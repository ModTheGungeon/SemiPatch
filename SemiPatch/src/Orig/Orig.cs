using System;
namespace SemiPatch {
    /// <summary> Represents a delegate to the original version of a non-void method. </summary>
    public delegate R Orig<R>();
    /// <summary> Represents a delegate to the original version of a non-void method with 1 parameter. </summary>
    public delegate R Orig<T1, R>(T1 p1);
    /// <summary> Represents a delegate to the original version of a non-void method with 2 parameters. </summary>
    public delegate R Orig<T1, T2, R>(T1 p1, T2 p2);
    /// <summary> Represents a delegate to the original version of a non-void method with 3 parameters. </summary>
    public delegate R Orig<T1, T2, T3, R>(T1 p1, T2 p2, T3 p3);
    /// <summary> Represents a delegate to the original version of a non-void method with 4 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, R>(T1 p1, T2 p2, T3 p3, T4 p4);
    /// <summary> Represents a delegate to the original version of a non-void method with 5 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5);
    /// <summary> Represents a delegate to the original version of a non-void method with 6 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6);
    /// <summary> Represents a delegate to the original version of a non-void method with 7 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7);
    /// <summary> Represents a delegate to the original version of a non-void method with 8 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8);
    /// <summary> Represents a delegate to the original version of a non-void method with 9 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9);
    /// <summary> Represents a delegate to the original version of a non-void method with 10 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10);
    /// <summary> Represents a delegate to the original version of a non-void method with 11 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11);
    /// <summary> Represents a delegate to the original version of a non-void method with 12 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12);
    /// <summary> Represents a delegate to the original version of a non-void method with 13 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13);
    /// <summary> Represents a delegate to the original version of a non-void method with 14 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14);
    /// <summary> Represents a delegate to the original version of a non-void method with 15 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15);
    /// <summary> Represents a delegate to the original version of a non-void method with 16 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16);
    /// <summary> Represents a delegate to the original version of a non-void method with 17 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17);
    /// <summary> Represents a delegate to the original version of a non-void method with 18 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18);
    /// <summary> Represents a delegate to the original version of a non-void method with 19 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19);
    /// <summary> Represents a delegate to the original version of a non-void method with 20 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20);
    /// <summary> Represents a delegate to the original version of a non-void method with 21 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21);
    /// <summary> Represents a delegate to the original version of a non-void method with 22 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22);
    /// <summary> Represents a delegate to the original version of a non-void method with 23 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23);
    /// <summary> Represents a delegate to the original version of a non-void method with 24 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24);
    /// <summary> Represents a delegate to the original version of a non-void method with 25 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25);
    /// <summary> Represents a delegate to the original version of a non-void method with 26 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25, T26 p26);
    /// <summary> Represents a delegate to the original version of a non-void method with 27 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25, T26 p26, T27 p27);
    /// <summary> Represents a delegate to the original version of a non-void method with 28 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25, T26 p26, T27 p27, T28 p28);
    /// <summary> Represents a delegate to the original version of a non-void method with 29 parameters. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25, T26 p26, T27 p27, T28 p28, T29 p29);
    /// <summary> Represents a delegate to the original version of a non-void method with 30 parameters. This is the maximum amount of parameters a ReceiveOriginal-tagged method can take. </summary>
    public delegate R Orig<T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12, T13, T14, T15, T16, T17, T18, T19, T20, T21, T22, T23, T24, T25, T26, T27, T28, T29, T30, R>(T1 p1, T2 p2, T3 p3, T4 p4, T5 p5, T6 p6, T7 p7, T8 p8, T9 p9, T10 p10, T11 p11, T12 p12, T13 p13, T14 p14, T15 p15, T16 p16, T17 p17, T18 p18, T19 p19, T20 p20, T21 p21, T22 p22, T23 p23, T24 p24, T25 p25, T26 p26, T27 p27, T28 p28, T29 p29, T30 p30);
}
