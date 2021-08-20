using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NV.DetectionPlatform.Service
{
    public enum ExamType
    {
        /// <summary>
        /// 单张
        /// </summary>
        Spot = 0,
        /// <summary>
        /// 动态
        /// </summary>
        Expose = Spot + 1,
        /// <summary>
        /// 多能单张
        /// </summary>
        MultiEnergyAvg = Expose + 1,
    }
}
