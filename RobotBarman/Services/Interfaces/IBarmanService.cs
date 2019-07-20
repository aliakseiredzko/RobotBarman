using System.Threading.Tasks;
using RozumConnectionLib;

namespace RobotBarman.Services
{
    public interface IBarmanService
    {
        int Speed { get; set; }
        DrinksPageItem SelectedBottle { get; set; }
        bool IsSpillRunning { get; set; }
        bool IsCanceledByTimer { get; set; }
        Position BaseCupPosition { get; set; }
        void ResetSpill();
        void ResetPutCupsToAgv();
        Task SpillAsync();
        Task PutCupAsync();
        Task PutCupsToAgv();
    }
}