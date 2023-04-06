using System;
using System.Collections.Generic;
using System.Linq;

public class ECBS
{
    // Class to represent the robot state (position and orientation)
    public class RobotState
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Orientation { get; set; } // 0: North, 1: East, 2: South, 3: West

        public RobotState(int x, int y, int orientation)
        {
            X = x;
            Y = y;
            Orientation = orientation;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            RobotState other = (RobotState)obj;
            return X == other.X && Y == other.Y && Orientation == other.Orientation;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(X, Y, Orientation).GetHashCode();
        }

        public override string ToString()
        {
            string[] orientations = new string[] { "North", "East", "South", "West" };
            return $"({X}, {Y}, {orientations[Orientation]})";
        }
    }


    // Class to represent a vertex in the graph
    public class Vertex
    {
        public RobotState State { get; set; }

        public Vertex(RobotState state)
        {
            State = state;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
            {
                return false;
            }

            Vertex other = (Vertex)obj;
            return State.Equals(other.State);
        }

        public override int GetHashCode()
        {
            return State.GetHashCode();
        }

        public override string ToString()
        {
            return State.ToString();
        }
    }


    // Class to represent an edge in the graph
    public class Edge
    {
        public Vertex Source { get; set; }
        public Vertex Destination { get; set; }
        public double Cost { get; set; }

        public Edge(Vertex source, Vertex destination, double cost)
        {
            Source = source;
            Destination = destination;
            Cost = cost;
        }
    }


    // Class to represent a constraint in the constraint tree
    public class Constraint
    {
        public int Agent { get; set; }
        public RobotState State { get; set; }
        public int TimeStep { get; set; }

        public Constraint(int agent, RobotState state, int timeStep)
        {
            Agent = agent;
            State = state;
            TimeStep = timeStep;
        }
    }


    public class Conflict
    {
        public int Agent1 { get; private set; }
        public int Agent2 { get; private set; }
        public RobotState State1 { get; private set; }
        public RobotState State2 { get; private set; }
        public int TimeStep1 { get; private set; }
        public int TimeStep2 { get; private set; }
        public bool IsEdgeConflict { get; private set; }

        public Conflict(int agent1, int agent2, RobotState state1, int timeStep1)
        {
            Agent1 = agent1;
            Agent2 = agent2;
            State1 = state1;
            State2 = state1;
            TimeStep1 = timeStep1;
            TimeStep2 = timeStep1;
            IsEdgeConflict = false;
        }

        public Conflict(int agent1, int agent2, RobotState state1, RobotState state2, int timeStep1, int timeStep2)
        {
            Agent1 = agent1;
            Agent2 = agent2;
            State1 = state1;
            State2 = state2;
            TimeStep1 = timeStep1;
            TimeStep2 = timeStep2;
            IsEdgeConflict = true;
        }
    }



    // Class to represent a node in the constraint tree
    public class CTNode
    {
        public List<Constraint> Constraints { get; set; }
        public List<List<RobotState>> Paths { get; set; }
        public double Cost { get; set; }

        public CTNode(List<Constraint> constraints, List<List<RobotState>> paths, double cost)
        {
            Constraints = constraints;
            Paths = paths;
            Cost = cost;
        }
    }


    // Graph representation
    private Dictionary<Vertex, List<Edge>> graph;

    // Constructor
    public ECBS() { /* ... */ }

    // Method to construct the graph from the input environment
    private void ConstructGraph() { /* ... */ }

    // Method to generate motion primitives for a given robot state
    private List<RobotState> GenerateMotionPrimitives(RobotState currentState)
    {
        List<RobotState> motionPrimitives = new List<RobotState>();

        // Motion primitives based on the current orientation
        int[][] directions = new int[4][] {
        new int[] { -1, 0 }, // North
        new int[] { 0, 1 }, // East
        new int[] { 1, 0 }, // South
        new int[] { 0, -1 } // West
    };

        // Forward movement
        int[] forward = directions[currentState.Orientation];
        motionPrimitives.Add(new RobotState(currentState.X + forward[0], currentState.Y + forward[1], currentState.Orientation));

        // Backward movement
        int[] backward = directions[(currentState.Orientation + 2) % 4];
        motionPrimitives.Add(new RobotState(currentState.X + backward[0], currentState.Y + backward[1], currentState.Orientation));

        // 90-degree turn (clockwise)
        int newOrientation = (currentState.Orientation + 1) % 4;
        motionPrimitives.Add(new RobotState(currentState.X, currentState.Y, newOrientation));

        // 90-degree turn (counterclockwise)
        newOrientation = (currentState.Orientation + 3) % 4;
        motionPrimitives.Add(new RobotState(currentState.X, currentState.Y, newOrientation));

        return motionPrimitives;
    }


    // Method to check for vertex and edge conflicts
    private List<Conflict> CheckConflicts(List<List<RobotState>> robotPaths)
    {
        List<Conflict> conflicts = new List<Conflict>();

        int maxPathLength = robotPaths.Max(path => path.Count);

        for (int t = 0; t < maxPathLength; t++)
        {
            for (int i = 0; i < robotPaths.Count; i++)
            {
                for (int j = i + 1; j < robotPaths.Count; j++)
                {
                    RobotState stateI = t < robotPaths[i].Count ? robotPaths[i][t] : robotPaths[i].Last();
                    RobotState stateJ = t < robotPaths[j].Count ? robotPaths[j][t] : robotPaths[j].Last();

                    // Check for vertex conflicts
                    if (stateI.Equals(stateJ))
                    {
                        conflicts.Add(new Conflict(i, j, stateI, t));
                    }

                    // Check for edge conflicts
                    if (t > 0)
                    {
                        RobotState prevStateI = t < robotPaths[i].Count ? robotPaths[i][t - 1] : robotPaths[i].Last();
                        RobotState prevStateJ = t < robotPaths[j].Count ? robotPaths[j][t - 1] : robotPaths[j].Last();

                        if (stateI.Equals(prevStateJ) && stateJ.Equals(prevStateI))
                        {
                            conflicts.Add(new Conflict(i, j, stateI, prevStateJ, t - 1, t));
                        }
                    }
                }
            }
        }

        return conflicts;
    }


    // Method to find the shortest path for an individual agent with constraints
    private List<RobotState> FindShortestPathWithConstraints(int agent, RobotState start, RobotState goal, List<Constraint> constraints)
    {
        Dictionary<RobotState, double> gScore = new Dictionary<RobotState, double>();
        Dictionary<RobotState, double> fScore = new Dictionary<RobotState, double>();
        Dictionary<RobotState, RobotState> cameFrom = new Dictionary<RobotState, RobotState>();

        gScore[start] = 0;
        fScore[start] = HeuristicCostEstimate(start, goal);

        SortedSet<(double, RobotState)> openSet = new SortedSet<(double, RobotState)>();
        openSet.Add((fScore[start], start));

        while (openSet.Count > 0)
        {
            RobotState current = openSet.First().Item2;
            openSet.Remove(openSet.First());

            if (current.Equals(goal))
            {
                return ReconstructPath(cameFrom, current);
            }

            List<RobotState> neighbors = GenerateMotionPrimitives(current);
            foreach (RobotState neighbor in neighbors)
            {
                if (IsValidTransition(agent, current, neighbor, constraints))
                {
                    double tentativeGScore = gScore[current] + 1; // Assuming equal cost for all transitions

                    if (!gScore.ContainsKey(neighbor) || tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = gScore[neighbor] + HeuristicCostEstimate(neighbor, goal);

                        if (!openSet.Any(item => item.Item2.Equals(neighbor)))
                        {
                            openSet.Add((fScore[neighbor], neighbor));
                        }
                    }
                }
            }
        }

        return null; // No path found
    }

    private bool IsValidTransition(int agent, RobotState currentState, RobotState nextState, List<Constraint> constraints)
    {
        //foreach (Constraint constraint in constraints)
        //{
        //    if (constraint.Agent == agent)
        //    {
        //        if (constraint.IsVertexConstraint && constraint.State.Equals(nextState) && constraint.TimeStep == nextState.TimeStep)
        //        {
        //            return false;
        //        }
        //        else if (constraint.IsEdgeConstraint &&
        //                 constraint.State.Equals(currentState) &&
        //                 constraint.NextState.Equals(nextState) &&
        //                 constraint.TimeStep == currentState.TimeStep)
        //        {
        //            return false;
        //        }
        //    }
        //}
        return true;
    }


    private double HeuristicCostEstimate(RobotState state, RobotState goal)
    {
        return Math.Abs(state.X - goal.X) + Math.Abs(state.Y - goal.Y);
    }

    private List<RobotState> ReconstructPath(Dictionary<RobotState, RobotState> cameFrom, RobotState current)
    {
        List<RobotState> path = new List<RobotState> { current };

        while (cameFrom.ContainsKey(current))
        {
            current = cameFrom[current];
            path.Insert(0, current);
        }

        return path;
    }


    // Method to implement the high-level ECBS algorithm
    public List<List<RobotState>> PlanMultiRobotTrajectories(List<RobotState> starts, List<RobotState> goals)
    {
        // Initialize the root of the constraint tree with no constraints
        CTNode rootNode = null;// new CTNode(new List<Constraint>());

        // Create a priority queue for the constraint tree nodes ordered by cost
        SortedSet<(double, CTNode)> ctQueue = new SortedSet<(double, CTNode)>();
        ctQueue.Add((0, rootNode));

        while (ctQueue.Count > 0)
        {
            CTNode currentNode = ctQueue.First().Item2;
            ctQueue.Remove(ctQueue.First());

            // Find individual shortest paths with current constraints
            List<List<RobotState>> paths = new List<List<RobotState>>();
            for (int agent = 0; agent < starts.Count; agent++)
            {
                List<Constraint> agentConstraints = currentNode.Constraints.FindAll(c => c.Agent == agent);
                paths.Add(FindShortestPathWithConstraints(agent, starts[agent], goals[agent], agentConstraints));
            }

            // Check for conflicts in the paths
            List<Conflict> conflicts = null;// CheckPaths(paths);

            if (conflicts.Count == 0)
            {
                // No conflicts, return the paths
                return paths;
            }
            else
            {
                // Resolve conflicts by adding new constraints to the constraint tree
                Conflict conflict = conflicts[0];
                List<CTNode> children = null;// currentNode.ResolveConflict(conflict);

                foreach (CTNode child in children)
                {
                    double cost = 0;// CalculateTotalPathCost(paths, child);
                    ctQueue.Add((cost, child));
                }
            }
        }

        return null; // No solution found
    }
}
