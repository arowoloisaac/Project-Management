namespace Task_Management_System.Models
{
    public class Pagination
    {
        public int Size { get; set; }// number of item in the particular page
        public int Current { get; set; }
        public int Count { get; set; } // number of pages
        public int Start { get; set; }
        public int End { get; set; }
        public int TotalItems { get; set; }

        public Pagination() { }

        public Pagination(int size, int current, int count, int start, int end, int totalItem)
        {
            this.Size = size;
            this.Start = start;
            this.End = end;
            this.Current = current;
            this.Count = count;
            this.TotalItems = totalItem;
        }
    }
}
