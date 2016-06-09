using System;

namespace genetics
{
	public class Genepool
	{	
		static Random random = new Random();

		double[,] pool;

		int poolMax;
		int geneMax;

		int index = 0;

		string filename;

		public Genepool (int _poolMax, int _geneMax)
		{
			poolMax = _poolMax;
			geneMax = _geneMax;

			pool = new double[poolMax,geneMax];

			for (int p = 0; p < poolMax; p++) {
				for (int g = 0; g < geneMax; g++) {
					pool [p,g] = (random.NextDouble () * 2) - 1;
				}
			}
		}

		void addGenes(double[] g)
		{
			for (int i = 0; i < g.Length; i++) {
				pool [(index++) % poolMax, i] = g [i];
			}
		}

		double[] getGenes()
		{
			double[] genesA = new double[geneMax];
			double[] genesB = new double[geneMax];
			int parentA = random.Next (0, poolMax);
			int parentB = random.Next (0, poolMax);

			for (int i = 0; i < geneMax; i++) {
				genesA [i] = pool [parentA, i];
			}

			double[] genesC = genesA;

			if (random.NextDouble() > 0.5) {
				return genesC; //Clone
			}

			if (random.NextDouble() > 0.5) {
				for (int i = 0; i < geneMax; i++) {
					genesB [i] = pool [parentB, i];
				}

				for (int i = 0; i < genesC.Length; i++) {
					if (random.NextDouble() > 0.5) {
						genesC[i] = genesB[i];
					}
				}
			}

			double mutate = random.NextDouble ();

			int chances = 0;

			while ((mutate < 0.5) && (chances < 10)) {
				int loci = random.Next (0, genesC.Length);
				if(mutate < 0.05)
					genesC[loci] = (random.NextDouble()*2)-1;
				else
				{
					genesC[loci] = (random.NextDouble()*0.2)-0.1;
					if (genesC [loci] > 1)
						genesC [loci] = 1;
					if (genesC [loci] < -1)
						genesC [loci] = -1;
				}

				mutate = random.NextDouble ();
				chances++;
			}

			return genesC;
		}
	}
}

