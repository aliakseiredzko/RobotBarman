using FreshMvvm;
using Mobile.Communication.Client;
using Mobile.Communication.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RobotBarman.Services
{
    public class AgvService : IAgvService
    {
        private RobotClient _robot;
        private readonly IJsonDatabaseService _jsonDatabaseService;
        private readonly IBarmanService _barmanService;
        
        public AgvService()
        {
            _jsonDatabaseService = FreshIOC.Container.Resolve<IJsonDatabaseService>();
            _barmanService = FreshIOC.Container.Resolve<IBarmanService>();
            _agv = _jsonDatabaseService.GetAgvData();

            var robotData = _jsonDatabaseService.GetAgvData();

            _robot = new RobotClient("Miron", IPAddress.Parse(robotData.Ip), robotData.Port);
        }

        private Agv _agv;

        public string Ip
        {
            get => _agv.Ip;
            set => _agv.Ip = value;
        }
        public int Port
        {
            get => _agv.Port;
            set => _agv.Port = value;
        }

        public string Password
        {
            get => _agv.Password;
            set => _agv.Password = value;
        }
        
        public bool IsConnected => _robot.Connected;

        public bool IsAgvBusy { get; set; }

        public string LastActionResult { get; protected set; }
        
        public IEnumerable<string> Goals { get; protected set; }
        
        public IEnumerable<string> Routes { get; protected set; }
        
        public string PutCupsGoal { get; set; }
        
        public string IntermediatePutCupsGoal { get; set; }
        
        public string RouteToPatrol { get; set; }

        public bool PatrolRouteForever { get; set; }

        private bool _isGoalReached;

        private bool _isActionFailed;

        private bool _isActionInterrupted;

        private string _reachedGoal;

        private string _failedAction;

        private string _interruptedAction;

        private void _robot_GoalReached(Robot robot, Mobile.Communication.Common.Event.GoalReachedEventArgs args)
        {
            _reachedGoal = args.Goal;
            _isGoalReached = true;
        }

        private void _robot_ActionFailed(Robot robot, Mobile.Communication.Common.Event.ActionEventArgs args)
        {
            _failedAction = args.Action;
            _isActionFailed = true;
        }

        private void _robot_ActionInterrupted(Robot robot, Mobile.Communication.Common.Event.ActionEventArgs args)
        {
            _interruptedAction = args.Action;
            _isActionInterrupted = true;
        }

        public async Task<ConnectionDetails> ConnectAsync()
        {            
            _agv = new Agv { Ip = Ip, Port = Port, Password = Password};
            _jsonDatabaseService.SaveAgvData(_agv);

            return await Task.Run(() =>
            {
                try
                {
                    _robot = new RobotClient("Miron", new IPEndPoint(IPAddress.Parse(_agv.Ip), _agv.Port))
                    {
                        Password = Password
                    };

                    _robot.Connect();

                    _robot.ActionFailed += _robot_ActionFailed;
                    _robot.GoalReached += _robot_GoalReached;
                    _robot.ActionInterrupted += _robot_ActionInterrupted;

                    return ConnectionDetails.Ok;
                }
                catch (InvalidOperationException e)
                {
                    return ConnectionDetails.WrongPassword;
                }
                catch (MobileException e)
                {
                    return ConnectionDetails.ConnectionError;
                }
            });
        }

        public async Task<bool> RefreshGoalsAsync()
        {
            return await Task.Run(() =>
            {
                bool result = _robot.RefreshGoals(out var goals, out var lastActionResult);
                LastActionResult = lastActionResult;
                Goals = goals;
                
                return result;
            });
        }

        public async Task<bool> RefreshRoutesAsync()
        {            
            return await Task.Run(() =>
            {
                bool result = _robot.RefreshRoutes(out var routes, out var lastActionResult);
                LastActionResult = lastActionResult;
                Routes = routes;
                
                return result;
            });
        }

        public async Task<bool> GoToGoalAsync(string goal, bool checkInterruptAction)
        {
            if (string.IsNullOrEmpty(goal)) return false;

            return await Task.Run(() =>
            {
                return GoToGoal(goal, checkInterruptAction);
            });
        }

        private bool GoToGoal(string goal, bool checkInterruptAction)
        {

            _isActionInterrupted = false;
            _isActionFailed = false;
            _isGoalReached = false;
            _failedAction = "";
            _interruptedAction = "";
            _reachedGoal = "";

            _robot.GoToGoal(goal, out var lastActionResult);
            LastActionResult = lastActionResult;

            WaitGoalAction();

            //if (_isActionInterrupted && _interruptedAction != goal && checkInterruptAction)
            //{
            //    _isActionInterrupted = false;
            //    return false;
            //}

            if (_isGoalReached && _reachedGoal == goal)
            {
                _isGoalReached = false;
                _reachedGoal = "";
                return true;
            }

            _isActionInterrupted = false;
            _isGoalReached = false;
            _interruptedAction = "";
            _reachedGoal = "";

            _isActionFailed = false;
            return false;
        }

        private void WaitGoalAction()
        {
            while (!_isActionFailed && !_isGoalReached && !_isActionInterrupted)
            {
                Thread.Sleep(50);
            }
        }

        public async Task<bool> PatrolRouteAsync(string route, bool forever)
        {
            return await Task.Run(() =>
            {
                bool result = _robot.PatrolRoute(route, forever, out var lastActionResult);
                LastActionResult = lastActionResult;

                return result;
            });
        }

        public async Task<bool> SayAsync(string speech)
        {
            return await Task.Run(() =>
            {
                bool result = _robot.Say(speech, out var lastActionResult);
                LastActionResult = lastActionResult;

                return result;
            });
        }

        public async Task<bool> ResetAsync()
        {
            return await Task.Run(() =>
            {
                bool result = _robot.Stop(out var lastActionResult);
                LastActionResult = lastActionResult;

                return result;
            });
        }

        public async Task<bool> PutCupsToAgv()
        {
            IsAgvBusy = true;

            try
            {
                _robot.Stop();
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
            }

            _isActionInterrupted = false;
            _isActionFailed = false;
            _isGoalReached = false;
            _failedAction = "";
            _interruptedAction = "";
            _reachedGoal = "";

            if (!string.IsNullOrEmpty(IntermediatePutCupsGoal))
            {
                if (!await GoToGoalAsync(IntermediatePutCupsGoal, false))
                {
                    IsAgvBusy = false;
                    return false;
                }
            }

            if (!await GoToGoalAsync(PutCupsGoal, true))
            {
                IsAgvBusy = false;
                return false;
            }

            await _barmanService.PutCupsToAgv();

            if (!string.IsNullOrEmpty(RouteToPatrol))
            {
                await PatrolRouteAsync(RouteToPatrol, PatrolRouteForever);
            }

            IsAgvBusy = false;

            return true;
        }
    }

    public enum ConnectionDetails
    {
        Ok, WrongPassword, ConnectionError
    }
    
    public static class AsyncExtensions
    {
        public static async Task<T> WithCancellation<T>(this Task<T> task, CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(s => ((TaskCompletionSource<bool>)s).TrySetResult(true), tcs))
            {
                if (task != await Task.WhenAny(task, tcs.Task))
                {
                    throw new OperationCanceledException(cancellationToken);
                }
            }

            return task.Result;
        }
    }
}
