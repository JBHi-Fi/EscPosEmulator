namespace ReceiptPrinterEmulator.EscPos.Commands;

public abstract class DataCommand : BaseCommand
{
    public override bool HasArgs => true;

    private int i = 0;
    private int pL = 0;
    private int pH = 0;
    protected byte m = 0;
    protected byte fn = 0;
    protected int di = 0;
    protected int dl = 0;
    protected byte[]? data = null;

    public override bool InterpretNextChar(byte c)
    {
        switch (i++)
        {
            case 0:
                pL = c;
                return true;
            case 1:
                pH = c;
                dl = (pH << 8) | pL - 2;
                data = dl > 0 ? new byte[dl] : null;
                return true;
            case 2:
                m = c;
                return true;
            case 3:
                fn = c;
                return dl > 0;
            default:
                data![di++] = c;
                return di < dl;
        }
    }

    public override void Reset()
    {
        i = 0;
        pL = 0;
        pH = 0;
        di = 0;
        dl = 0;
        m = 0;
        fn = 0;
        data = null;
    }
}
