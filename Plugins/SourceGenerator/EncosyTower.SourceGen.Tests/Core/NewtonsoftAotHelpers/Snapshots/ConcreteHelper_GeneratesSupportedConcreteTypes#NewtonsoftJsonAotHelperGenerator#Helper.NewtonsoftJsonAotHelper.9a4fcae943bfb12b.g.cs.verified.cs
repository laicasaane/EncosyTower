#pragma warning disable 0219

using System.Collections.Generic;
using EncosyTower.Serialization.NewtonsoftJson;

#pragma warning disable CS0105 // Using directive appeared previously in this namespace

using g__NSJU = global::Newtonsoft.Json.Utilities;
using g__UES = global::UnityEngine.Scripting;

#pragma warning restore CS0105 // Using directive appeared previously in this namespace


namespace TestProject
{



#pragma warning disable

static partial class Helper
{
    [g__UES.Preserve]
    public static void EnsureNewtonsoftJson()
    {
        g__NSJU.AotHelper.EnsureType<global::TestProject.BaseModel>();
        {
        }
        g__NSJU.AotHelper.EnsureType<global::TestProject.ConcreteDerived>();
        {
            g__NSJU.AotHelper.EnsureType<global::System.Collections.Generic.List<global::TestProject.ConcreteDerived>>();
            g__NSJU.AotHelper.EnsureType<global::TestProject.ConcreteDerived>();
            g__NSJU.AotHelper.EnsureList<global::TestProject.ConcreteDerived>();
        }
    }

}



}

