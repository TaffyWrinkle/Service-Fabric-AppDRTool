﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace RestoreService
{
    class Utility
    {
        public static object CloneObject(object objSource)
        {
            //step : 1 Get the type of source object and create a new instance of that type
            Type typeSource = objSource.GetType();
            object objTarget = Activator.CreateInstance(typeSource);
            //Step2 : Get all the properties of source object type
            PropertyInfo[] propertyInfo = typeSource.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            //Step : 3 Assign all source property to taget object 's properties
            foreach (PropertyInfo property in propertyInfo)
            {
                //Check whether property can be written to
                if (property.CanWrite)
                {
                    //Step : 4 check whether property type is value type, enum or string type
                    if (property.PropertyType.IsValueType || property.PropertyType.IsEnum || property.PropertyType.Equals(typeof(System.String)))
                    {
                        property.SetValue(objTarget, property.GetValue(objSource, null), null);
                    }
                    //else property type is object/complex types, so need to recursively call this method until the end of the tree is reached
                    else
                    {
                        object objPropertyValue = property.GetValue(objSource, null);
                        if (objPropertyValue == null)
                        {
                            property.SetValue(objTarget, null, null);
                        }
                        else
                        {
                            property.SetValue(objTarget, CloneObject(objPropertyValue), null);
                        }
                    }
                }
            }
            return objTarget;
        }

        public static T DeepClone<T>(T obj)
        {
            using (var ms = new MemoryStream())
            {
                var formatter = new BinaryFormatter();
                formatter.Serialize(ms, obj);
                ms.Position = 0;

                return (T)formatter.Deserialize(ms);
            }
        }

        public static String getPartitionAccessKey(Guid partitionId, String primaryClusterName, String secondaryClusterName)
        {
            return getPrimarySecondaryClusterJoin(primaryClusterName, secondaryClusterName) + "~" + partitionId.ToString();
        }

        public static bool isPartitionFromPrimarySecondaryCombination(String partitionAccessKey, String primaryClusterName, String secondaryClusterName)
        {
            string[] parts = partitionAccessKey.Split('~');
            String psc = getPrimarySecondaryClusterJoin(primaryClusterName, secondaryClusterName);
            return psc.Equals(parts[0]);
        }

        public static String getPrimarySecondary(String partitionAccessKey)
        {
            return partitionAccessKey.Split('~')[0];
        }

        public static String getPrimarySecondaryClusterJoin(String primaryClusterName, String secondaryClusterName)
        {
            return primaryClusterName + ":" + secondaryClusterName;
        }

        public static String getClusterNameFromTCPEndpoint(String clusterEndpoint)
        {
            return clusterEndpoint.Split(':')[0];
        }
    }
}
