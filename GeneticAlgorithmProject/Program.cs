using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text;

public class Program
{
    public static void Main(string[] args)
    {
        string inputFilePath = "C:\\Users\\AdhOom\\Documents\\GitHub\\GeneticSharp\\data.csv"; 
        string outputFilePath = "C:\\Users\\AdhOom\\Documents\\GitHub\\GeneticSharp\\dataoutput.csv"; 

        var data = DataReader.ReadCsv(inputFilePath);
        if (data == null || data.Count == 0) return;

        int populationSize = 100;
        double mutationRate = 0.05;
        int maxGenerations = 1000;

        var ga = new GeneticAlgorithm(populationSize, mutationRate, data);

        Console.WriteLine($"Starting GA to find {data.First().Inputs.Length} positive weights...");
        
        ga.Evolve(maxGenerations);

        Console.WriteLine("\n--- Optimal Solution Found ---");
        Console.WriteLine($"Best Fitness Score: {ga.BestIndividual.Fitness:F8}"); // يبقى رقم عشري
        
        // طباعة تمثيل أفضل كروموسوم (أرقام عشرية موجبة)
        var formattedGenes = ga.BestIndividual.Genes.Select(g => g.ToString("F4"));
        string geneRepresentation = string.Join(" | ", formattedGenes);

        Console.WriteLine($"Chromosome Weights (W1 | W2 | ...): {geneRepresentation}");
        
        WriteSolutionToCsv(ga.BestIndividual, outputFilePath);
        Console.WriteLine($"\nOptimal weights saved to: {outputFilePath}");
    }

    private static void WriteSolutionToCsv(Individual bestIndividual, string filePath)
    {
        var sb = new StringBuilder();
        
        sb.AppendLine("Metric,Value");
        sb.AppendLine($"Best Fitness,{bestIndividual.Fitness}");
        sb.AppendLine($"Gene Count,{bestIndividual.Genes.Length}");
        
        sb.AppendLine(); 
        
        string header = "Gene/Weight";
        for (int i = 0; i < bestIndividual.Genes.Length; i++)
        {
            header += $",W{i + 1}";
        }
        sb.AppendLine(header);
        
        string values = "Value (0 to 1)";
        foreach (var gene in bestIndividual.Genes)
        {
            values += $",{gene.ToString("R")}"; 
        }
        sb.AppendLine(values);

        File.WriteAllText(filePath, sb.ToString());
    }
}

// ====================================================================
// A. قراءة البيانات (DataReader, DataRow) - لا تعديل
// ====================================================================

public class DataRow
{
    public double[] Inputs { get; set; } 
    public double Target { get; set; }   
}

public static class DataReader
{
    public static List<DataRow> ReadCsv(string filePath)
    {
        var allRows = new List<DataRow>();
        try
        {
            var lines = File.ReadAllLines(filePath).Skip(1); 
            foreach (var line in lines)
            {
                var values = line.Split(','); 
                if (values.Length < 2) continue;
                double target = double.Parse(values.Last());
                var inputs = values.Take(values.Length - 1).Select(double.Parse).ToArray();
                allRows.Add(new DataRow { Inputs = inputs, Target = target });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error reading CSV file: {ex.Message}");
            return null;
        }
        return allRows;
    }
}

// ====================================================================
// B. الكروموسوم (Individual) واللياقة (Fitness) - **تم التعديل**
// ====================================================================

public class Individual
{
    public double[] Genes { get; private set; } 
    public double Fitness { get; set; }        
    private static readonly Random Rng = new Random();

    public Individual(int geneCount)
    {
        Genes = new double[geneCount];
        // التهيئة الآن بين 0.0 و 1.0 فقط
        for (int i = 0; i < geneCount; i++)
        {
            Genes[i] = Rng.NextDouble(); 
        }
    }
    
    public Individual(double[] genes)
    {
        Genes = genes;
    }

    public Individual Clone()
    {
        return new Individual((double[])Genes.Clone());
    }
}

public class Fitness
{
    private readonly List<DataRow> _data;

    public Fitness(List<DataRow> data)
    {
        _data = data;
    }

    public void Evaluate(Individual individual)
    {
        double[] weights = individual.Genes; 
        double totalSquaredError = 0;

        foreach (var row in _data)
        {
            double predictedValue = 0;
            
            for (int i = 0; i < weights.Length; i++)
            {
                // التأكد من أن الوزن موجب فقط (لغرض الحساب)
                double positiveWeight = Math.Max(0, weights[i]);
                predictedValue += row.Inputs[i] * positiveWeight;
            }

            double error = row.Target - predictedValue;
            totalSquaredError += error * error; 
        }

        individual.Fitness = 1.0 / (1.0 + totalSquaredError);
    }
}

// ====================================================================
// C. الخوارزمية الجينية (GeneticAlgorithm) - **تم التعديل**
// ====================================================================

public class GeneticAlgorithm
{
    private List<Individual> Population;
    private readonly Fitness FitnessEvaluator;
    private readonly int PopulationSize;
    private readonly double MutationRate;
    private readonly Random Rng = new Random();

    public Individual BestIndividual { get; private set; }

    public GeneticAlgorithm(int popSize, double mutationRate, List<DataRow> data)
    {
        PopulationSize = popSize;
        MutationRate = mutationRate;
        FitnessEvaluator = new Fitness(data);
        
        Population = new List<Individual>();
        int geneCount = data.First().Inputs.Length;
        for (int i = 0; i < popSize; i++)
        {
            Population.Add(new Individual(geneCount));
        }
    }

    public void Evolve(int maxGenerations)
    {
        for (int generation = 0; generation < maxGenerations; generation++)
        {
            foreach (var individual in Population)
            {
                FitnessEvaluator.Evaluate(individual);
            }
            
            Population = Population.OrderByDescending(i => i.Fitness).ToList();
            BestIndividual = Population.First();

            Console.WriteLine($"Generation {generation + 1}: Best Fitness = {BestIndividual.Fitness:F8}");

            if (BestIndividual.Fitness > 0.99999) break; 

            var nextGeneration = new List<Individual>();
            
            nextGeneration.Add(BestIndividual.Clone()); 

            while (nextGeneration.Count < PopulationSize)
            {
                Individual parent1 = TournamentSelection(5); 
                Individual parent2 = TournamentSelection(5);
                
                Individual offspring = Crossover(parent1, parent2);
                
                Mutate(offspring);

                nextGeneration.Add(offspring);
            }

            Population = nextGeneration;
        }
    }
    
    private Individual TournamentSelection(int k)
    {
        Individual best = null;
        for (int i = 0; i < k; i++)
        {
            int randomIndex = Rng.Next(PopulationSize);
            Individual contestant = Population[randomIndex];
            
            if (best == null || contestant.Fitness > best.Fitness)
            {
                best = contestant;
            }
        }
        return best;
    }

    private Individual Crossover(Individual parent1, Individual parent2)
    {
        int point = Rng.Next(1, parent1.Genes.Length - 1); 
        int geneCount = parent1.Genes.Length;
        double[] newGenes = new double[geneCount];

        for (int i = 0; i < geneCount; i++)
        {
            newGenes[i] = (i < point) ? parent1.Genes[i] : parent2.Genes[i];
        }

        return new Individual(newGenes); 
    }
    
    // الطفرة: الحصر ضمن [0.0, 1.0] لضمان الأرقام العشرية الموجبة
    private void Mutate(Individual individual)
    {
        for (int i = 0; i < individual.Genes.Length; i++)
        {
            if (Rng.NextDouble() < MutationRate)
            {
                // تغيير عشوائي صغير
                individual.Genes[i] += (Rng.NextDouble() * 2.0 - 1.0) * 0.5;
                
                // الحصر ضمن النطاق المطلوب: [0.0, 1.0]
                individual.Genes[i] = Math.Clamp(individual.Genes[i], 0.0, 1.0);
            }
        }
    }
}