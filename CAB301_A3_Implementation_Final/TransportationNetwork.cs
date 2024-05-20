//2024 CAB301 Assignment 3 
//TransportationNetwok.cs
//Assignment3B-TransportationNetwork

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Security.Principal;

public partial class TransportationNetwork
{

    private string[]? intersections; //array storing the names of those intersections in this transportation network design
    private int[,]? distances; //adjecency matrix storing distances between each pair of intersections, if there is a road linking the two intersections

    public string[]? Intersections
    {
        get { return intersections; }
    }

    public int[,]? Distances
    {
        get { return distances; }
    }


    //Read information about a transportation network plan into the system
    //Preconditions: The given file exists at the given path and the file is not empty
    //Postconditions: Return true, if the information about the transportation network plan is read into the system, 
    //the intersections are stored in the class field, intersections, and the distances of the links between the 
    //intersections are stored in the class fields, distances; otherwise, return false and both intersections and distances are null. 
    public bool ReadFromFile(string filePath)
    {
        bool successful = false;
        // check if the file exists
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath); // get an array of the intersections
            // if the data is good, go onto create the distances matrix
            if (SaveIntersections(lines))
            {
                SaveDistances(lines);
                successful = true;
            }
        }
        return successful;
    }
    // Save the intersections to the class field
    private bool SaveIntersections(string[] intersectionsList)
    {
        // make a new set of strings to avoid duplicates
        HashSet<string> intersectionsSet = new HashSet<string>();
        int count = 0;
        // loop through each intersection, check it, then add if it's valid
        foreach (string intersection in intersectionsList)
        {
            count++;
            string[] intersectionElements = intersection.Split(", ");
            // check the intersection
            if (!CheckIntersection(intersectionElements, count)) return false;
            // if ok, add the intersection to our set
            for (int i = 0; i < 2; i++) intersectionsSet.Add(intersectionElements[i]);
        }
        // sort the intersections and add them
        intersections = intersectionsSet.ToArray();
        Array.Sort(intersections);
        return true;
    }

    // Check each line of the file and find anything wrong
    private bool CheckIntersection(string[] lineElements, int count)
    {
        // if they're missing data
        if (lineElements.Length != 3)
        {
            Console.WriteLine($"Invalid format in line: {count}.");
            return false;
        }
        // if they haven't given a distance for the intersection
        else if (!int.TryParse(lineElements[2], out _))
        {
            Console.WriteLine($"Invalid distance in line: {count}.");
            return false;
        }
        return true;
    }

    // Save the distances to the class field
    private void SaveDistances(string[] intersectionsList)
    {
        // create our empty distances matrix
        distances = new int[Intersections.Length, Intersections.Length];

        // go through each intersection and update the matrix
        foreach (string intersection in intersectionsList)
        {
            string[] interSectionElements = intersection.Split(", ");
            // update the adjacency matrix based of the index of the first and second element of the intersection in the Intersections array
            distances[Array.IndexOf(Intersections, interSectionElements[0]), Array.IndexOf(Intersections, interSectionElements[1])] = int.Parse(interSectionElements[2]);
        }
        // using 999 to represent infinity since Int.MaxValue caused errors
        // because Int32.MaxValue caused overflow errors
        for (int i = 0; i < distances.GetLength(0); i++)
        {
            for (int j = 0; j < distances.GetLength(1); j++)
            {
                if (distances[i, j] == 0) distances[i, j] = 999;
            }
        }
    }


    //Display the transportation network plan with intersections and distances between intersections
    //Preconditions: The given file exists at the given path and the file is not empty
    //Postconditions: The transportation netork is displayed in a matrix format
    public void DisplayTransportNetwork()
    {
        Console.Write("       ");
        for (int i = 0; i < intersections?.Length; i++)
        {
            Console.Write(intersections[i].ToString().PadRight(5) + "  ");
        }
        Console.WriteLine();


        for (int i = 0; i < distances?.GetLength(0); i++)
        {
            Console.Write(intersections[i].ToString().PadRight(5) + "  ");
            for (int j = 0; j < distances?.GetLength(1); j++)
            {
                if (distances[i, j] == 999)
                    Console.Write("INF  " + "  ");
                else
                    Console.Write(distances[i, j].ToString().PadRight(5) + "  ");
            }
            Console.WriteLine();
        }
    }


    //Check if this transportation network is strongly connected. A transportation network is strongly connected, if there is a path from any intersection to any other intersections in thihs transportation network. 
    //Precondition: Transportation network plan data have been read into the system.
    //Postconditions: return true, if this transpotation netork is strongly connected; otherwise, return false. This transportation network remains unchanged.
    public bool IsConnected()
    {
        // use Floyd's algorithm to find all possible paths from all the vertices
        int[,] floydsMatrix = DoFloydsAlgorithm();

        for (int i = 0; i < floydsMatrix.GetLength(0); i++)
        {
            for (int j = 0; j < floydsMatrix.GetLength(1); j++)
            {
                // if any of the paths are 999, it means there is no way to get
                // to that node, so the graph isn't strongly connected
                if (floydsMatrix[i, j] == 999) return false;
            }
        }

        return true;
    }
    // use Floyd's algorithm to find the shortest distance between all the nodes
    // if there isn't one, it'll be 999
    private int[,] DoFloydsAlgorithm()
    {
        int[,] D = new int[Intersections.Length, Intersections.Length];
        Array.Copy(Distances, D, Distances.Length);
        for (int k = 0; k < Intersections.Length; k++)
        {
            for (int i = 0; i < Intersections.Length; i++)
            {
                for (int j = 0; j < Intersections.Length; j++)
                {
                    D[i, j] = Math.Min(D[i, j], D[i, k] + D[k, j]);
                }
            }
        }

        return D;
    }

    //Find the shortest path between a pair of intersections
    //Precondition: transportation network plan data have been read into the system
    //Postcondition: return the shorest distance between two different intersections; return 0 if there is no path from startVerte to endVertex; returns -1 if startVertex or endVertex does not exists. This transportation network remains unchanged.

    public int FindShortestDistance(string startVertex, string endVertex)
    {
        if (!Intersections.Contains(startVertex) || !Intersections.Contains(endVertex)) return -1;

        int[,] floydsMatrix = DoFloydsAlgorithm();
        int startIndex = Array.IndexOf(Intersections, startVertex);
        int endIndex = Array.IndexOf(Intersections, endVertex);

        if (floydsMatrix[startIndex, endIndex] == 999) return 0;
        else return floydsMatrix[startIndex, endIndex];
    }


    //Find the shortest path between all pairs of intersections
    //Precondition: transportation network plan data have been read into the system
    //Postcondition: return the shorest distances between between all pairs of intersections through a two-dimensional int array and this transportation network remains unchanged

    public int[,] FindAllShortestDistances()
    {
        return DoFloydsAlgorithm();
    }
}