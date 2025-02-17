using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GenerativeDesign.GenerativeDesign.Car
{
    public abstract class Distribution
    {
        public abstract (List<T> values, List<float> probabilities) GetDistribution<T>();
    }
}
