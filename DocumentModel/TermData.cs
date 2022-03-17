namespace DocumentModel;
//
// Summary:
//     Class with the differents attributes of terms to be considered
//
public class TermData
{
    //The TF, that is Term frequency/max term frequency
    public double TF { get; set; }

    //The weigth calculated according to the tf-idf formula
    public double Weigth { get; set; }

    //The frequency of the Term
    public int Frequency;

    //The positions on which a term appears
    public List<int> Positions;


    //
    // Summary: 
    //      Constructor of the class
    //
    public TermData(double tf = 0, int frequency = 0)
    {
        this.TF = tf;
        this.Frequency = frequency;
        this.Positions = new List<int>();
    }


    //
    // Summary: 
    //      Adds the position to the position list
    //
    public void AddPos(int position)
    {
        this.Positions.Add(position);

    }

}