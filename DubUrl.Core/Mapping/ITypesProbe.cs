﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DubUrl.Mapping;
public interface ITypesProbe
{
    IEnumerable<Type> Locate();
}
