using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace PADI_MapNoReduce
{
  interface IMap
  {
      public Set<String key, String value> Map(String key, String value);
  }
}
