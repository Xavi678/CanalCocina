using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CanalCocina3
{
	class Class1
	{
		public static void Main(string[] args)
		{
			CanalCocina cl = new CanalCocina();
			DateTime e = DateTime.Now;
			List<Program> llistaP = new List<Program>();
			llistaP=cl.Get(e);
			
			for(int i = 0; i < llistaP.Count; i++)
			{
				Console.WriteLine("args1: {0}", llistaP[i]);
			}
			
		}
	}
}
