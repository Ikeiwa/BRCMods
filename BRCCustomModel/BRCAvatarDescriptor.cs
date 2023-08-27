using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BRCCustomModel
{
    public class BRCAvatarDescriptor : MonoBehaviour
    {
        public SkinnedMeshRenderer blinkRenderer;
        public string blinkBlendshape = null;
        public int character = -1;
        public Material[] skins;
    }
}
