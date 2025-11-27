namespace FinalAspNetProj.DTO
{
    public class CountDTO
    {
        public int Count { get; set; }
    }

    public class ScoreDTO
    {
        public decimal Score { get; set; }
    }

    public class PercentageDTO
    {
        public int Percentage { get; set; }
    }

    public class ChartDataDTO
    {
        public List<string> Labels { get; set; } = new List<string>();
        public List<ChartDataset> Datasets { get; set; } = new List<ChartDataset>();
    }

    public class ChartDataset
    {
        public string Label { get; set; } = null!;
        public List<int> Data { get; set; } = new List<int>();
    }
}