using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

using OSIsoft.AF;
using OSIsoft.AF.Asset;
using OSIsoft.AF.PI;

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {

            if ( args.Length < 2 )
            {
                Console.WriteLine("Please input at least 2 arguments : <AFServer> <DatabaseName> [PointSource]");
                Environment.Exit(0);
            }
            PISystems myPISystems = new PISystems();
            PISystem myPISystem = myPISystems[args[0]];
            AFDatabase myAFDatabase = myPISystem.Databases[args[1]];

            PIServers myPIServers = new PIServers();
            PIServer myPIServer = myPIServers.DefaultPIServer;
            myPIServer.Connect();

            IEnumerable<PIPoint> ptlist = PIPoint.FindPIPoints(myPIServer, "*");
            PIPointList myPIPointList = new PIPointList(ptlist);
            IList<PIPoint> AFPIPointList = new List<PIPoint>();

            AFAttributeList myAttrs = AFAttribute.FindElementAttributes(
                    myAFDatabase,
                    null,
                    null,
                    null,
                    null,
                    AFElementType.Any,
                    null,
                    null,
                    System.TypeCode.Empty,
                    true,
                    AFSortField.Name,
                    AFSortOrder.Ascending,
                    int.MaxValue
                );
            
            foreach(AFAttribute myAttr in myAttrs)
            {
                try
                {
                    if (myAttr.PIPoint != null)
                    {
                        AFPIPointList.Add(myAttr.PIPoint);
                    }

                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString()+"\nAttribute Path : " + myAttr.GetPath() + "\n");
                }

            }

            PIPointList result = new PIPointList(myPIPointList.Except(AFPIPointList));

            Console.WriteLine("Discovered " + result.Count() + " tags not referenced in an AF Attribute");

            if(args.Length == 3)    //If the user defines a Pointsource
            {
                Console.WriteLine("Filtering with PointSource = " + args[2].ToUpper());
            }
            
            result.LoadAttributes("pointsource");
            foreach (PIPoint a in result)
            {
                if (args.Length == 3)
                {
                    if (a.GetAttribute("pointsource").ToString().ToUpper() == args[2].ToUpper())
                    {
                        Console.WriteLine(a.Name);
                    }
                }
                else
                {
                    Console.WriteLine(a.Name);
                }
                
            }

        }
    }
}
