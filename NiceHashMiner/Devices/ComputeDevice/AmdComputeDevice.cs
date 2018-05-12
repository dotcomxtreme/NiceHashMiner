﻿using ATI.ADL;
using NiceHashMiner.Enums;
using System;

namespace NiceHashMiner.Devices
{
    public class AmdComputeDevice : ComputeDevice
    {
        private readonly int _adapterIndex; // For ADL
        private static IntPtr _adlContext;

        public override int FanSpeed
        {
            get
            {
                var adlf = new ADLFanSpeedValue
                {
                    SpeedType = ADL.ADL_DL_FANCTRL_SPEED_TYPE_RPM
                };
                var result = ADL.ADL_Overdrive5_FanSpeed_Get(_adapterIndex, 0, ref adlf);
                if (result != ADL.ADL_SUCCESS)
                {
                    Helpers.ConsolePrint("ADL", "ADL fan getting failed with error code " + result);
                    return -1;
                }
                return adlf.FanSpeed;
            }
        }

        public override float Temp
        {
            get
            {
                var adlt = new ADLTemperature();
                var result = ADL.ADL_Overdrive5_Temperature_Get(_adapterIndex, 0, ref adlt);
                if (result != ADL.ADL_SUCCESS)
                {
                    Helpers.ConsolePrint("ADL", "ADL temp getting failed with error code " + result);
                    return -1;
                }
                return adlt.Temperature * 0.001f;
            }
        }

        public override float Load
        {
            get
            {
                var adlp = new ADLPMActivity();
                var result = ADL.ADL_Overdrive5_CurrentActivity_Get(_adapterIndex, ref adlp);
                if (result != ADL.ADL_SUCCESS)
                {
                    Helpers.ConsolePrint("ADL", "ADL load getting failed with error code " + result);
                    return -1;
                }
                return adlp.ActivityPercent;
            }
        }

        public override double PowerUsage
        {
            get
            {
                var power = -1;
                var result = ADL.ADL2_Overdrive6_CurrentPower_Get(_adlContext, _adapterIndex, 0, ref power);
                if (result != ADL.ADL_SUCCESS)
                {
                    Helpers.ConsolePrint("ADL", "ADL power getting failed with code " + result);
                }

                return power;
            }
        }

        public AmdComputeDevice(AmdGpuDevice amdDevice, int gpuCount, bool isDetectionFallback)
            : base(amdDevice.DeviceID,
                amdDevice.DeviceName,
                true,
                DeviceGroupType.AMD_OpenCL,
                amdDevice.IsEtherumCapable(),
                DeviceType.AMD,
                string.Format(International.GetText("ComputeDevice_Short_Name_AMD_GPU"), gpuCount),
                amdDevice.DeviceGlobalMemory)
        {
            Uuid = isDetectionFallback
                ? GetUuid(ID, GroupNames.GetGroupName(DeviceGroupType, ID), Name, DeviceGroupType)
                : amdDevice.UUID;
            BusID = amdDevice.BusID;
            Codename = amdDevice.Codename;
            InfSection = amdDevice.InfSection;
            AlgorithmSettings = GroupAlgorithms.CreateForDeviceList(this);
            DriverDisableAlgos = amdDevice.DriverDisableAlgos;
            Index = ID + ComputeDeviceManager.Avaliable.AvailCpus + ComputeDeviceManager.Avaliable.AvailNVGpus;
            _adapterIndex = amdDevice.AdapterIndex;
            
            if (_adlContext == default(IntPtr))
            {
                ADL.ADL2_Main_Control_Create(ADL.ADL_Main_Memory_Alloc, 0, ref _adlContext);
            }
        }
    }
}
