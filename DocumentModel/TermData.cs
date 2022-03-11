namespace DocumentModel;
public class TermData
{
    public double TF { get; set; }
    public double Weigth { get; set; }

    public int frequency;

    public List<int> Positions;

    public TermData(double tf = 0, int frequency = 0)
    {
        this.TF = tf;
        this.frequency = frequency;
        this.Positions = new List<int>();
    }

    public void AddPos(int position)
    {
        this.Positions.Add(position);

    }

}