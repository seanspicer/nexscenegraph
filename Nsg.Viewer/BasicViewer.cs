using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Veldrid.Sdl2;
using Vulkan;
using static Vulkan.VulkanNative;
using Nsg.Viewer.Internal;

namespace Nsg.Viewer
{
     public unsafe class BasicViewer
     {
         public string Name
         {
             get => InternalName.ToString();
             set => InternalName = value;
         }

         public VkInstance Instance { get; protected set; }
         
         private Sdl2Window NativeWindow { get; set; }
         private FixedUtf8String InternalName { get; set; } = "BasicViewer Window";
         private Settings Settings { get; } = new Settings();
         
         public VkPhysicalDevice PhysicalDevice { get; protected set; }
         internal vksVulkanDevice vulkanDevice { get; set; }
         public VkPhysicalDeviceProperties DeviceProperties { get; protected set; }
         public VkPhysicalDeviceFeatures DeviceFeatures { get; protected set; }
         public VkPhysicalDeviceMemoryProperties DeviceMemoryProperties { get; set; }
         public VkPhysicalDeviceFeatures enabledFeatures { get; protected set; }
         internal NativeList<IntPtr> EnabledExtensions { get; } = new NativeList<IntPtr>();
         public VkDevice device { get; protected set; }
         public VkQueue queue { get; protected set; }
         public VkFormat DepthFormat { get; protected set; }
         internal VulkanSwapchain Swapchain { get; } = new VulkanSwapchain();
         
         private NativeList<Semaphores> semaphores = new NativeList<Semaphores>(1, 1);
         private Semaphores* GetSemaphoresPtr() => (Semaphores*)semaphores.GetAddress(0);
         
         internal VkSubmitInfo submitInfo;
         internal NativeList<VkPipelineStageFlags> submitPipelineStages = CreateSubmitPipelineStages();
         private static NativeList<VkPipelineStageFlags> CreateSubmitPipelineStages()
             => new NativeList<VkPipelineStageFlags>() { VkPipelineStageFlags.ColorAttachmentOutput };
         
         public BasicViewer()
         {
             InitVulkan();
             CreateWindow();
         }

         public void Show()
         {
             NativeWindow.Visible = true;
             while (NativeWindow.Exists)
             {
                 NativeWindow.PumpEvents();
             }
         }

         private void InitVulkan()
         {
             VkResult err;
             err = CreateInstance(false);
             if (err != VkResult.Success)
             {
                 throw new InvalidOperationException("Could not create Vulkan instance. Error: " + err);
             }
             
             if (Settings.Validation)
            {

            }

            // Physical Device
            uint gpuCount = 0;
            Util.CheckResult(vkEnumeratePhysicalDevices(Instance, &gpuCount, null));
            Debug.Assert(gpuCount > 0);
            // Enumerate devices
            IntPtr* physicalDevices = stackalloc IntPtr[(int)gpuCount];
            err = vkEnumeratePhysicalDevices(Instance, &gpuCount, (VkPhysicalDevice*)physicalDevices);
            if (err != VkResult.Success)
            {
                throw new InvalidOperationException("Could not enumerate physical devices.");
            }

            // GPU selection

            // Select physical Device to be used for the Vulkan example
            // Defaults to the first Device unless specified by command line

            uint selectedDevice = 0;
            // TODO: Implement arg parsing, etc.

            PhysicalDevice = ((VkPhysicalDevice*)physicalDevices)[selectedDevice];

            // Store properties (including limits) and features of the phyiscal Device
            // So examples can check against them and see if a feature is actually supported
            VkPhysicalDeviceProperties deviceProperties;
            vkGetPhysicalDeviceProperties(PhysicalDevice, &deviceProperties);
            DeviceProperties = deviceProperties;

            VkPhysicalDeviceFeatures deviceFeatures;
            vkGetPhysicalDeviceFeatures(PhysicalDevice, &deviceFeatures);
            DeviceFeatures = deviceFeatures;

            // Gather physical Device memory properties
            VkPhysicalDeviceMemoryProperties deviceMemoryProperties;
            vkGetPhysicalDeviceMemoryProperties(PhysicalDevice, &deviceMemoryProperties);
            DeviceMemoryProperties = deviceMemoryProperties;

            // Derived examples can override this to set actual features (based on above readings) to enable for logical device creation
            //getEnabledFeatures();

            // Vulkan Device creation
            // This is handled by a separate class that gets a logical Device representation
            // and encapsulates functions related to a Device
            vulkanDevice = new vksVulkanDevice(PhysicalDevice);
            VkResult res = vulkanDevice.CreateLogicalDevice(enabledFeatures, EnabledExtensions);
            if (res != VkResult.Success)
            {
                throw new InvalidOperationException("Could not create Vulkan Device.");
            }
            device = vulkanDevice.LogicalDevice;

            // Get a graphics queue from the Device
            VkQueue queue;
            vkGetDeviceQueue(device, vulkanDevice.QFIndices.Graphics, 0, &queue);
            this.queue = queue;

            // Find a suitable depth format
            VkFormat depthFormat;
            uint validDepthFormat = Tools.getSupportedDepthFormat(PhysicalDevice, &depthFormat);
            Debug.Assert(validDepthFormat == True);
            DepthFormat = depthFormat;

            Swapchain.Connect(Instance, PhysicalDevice, device);

            // Create synchronization objects
            VkSemaphoreCreateInfo semaphoreCreateInfo = Initializers.semaphoreCreateInfo();
            // Create a semaphore used to synchronize image presentation
            // Ensures that the image is displayed before we start submitting new commands to the queu
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->PresentComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands have been sumbitted and executed
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->RenderComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands for the text overlay have been sumbitted and executed
            // Will be inserted after the render complete semaphore if the text overlay is enabled
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->TextOverlayComplete));

            // Set up submit info structure
            // Semaphores will stay the same during application lifetime
            // Command buffer submission info is set by each example
            submitInfo = Initializers.SubmitInfo();
            submitInfo.pWaitDstStageMask = (VkPipelineStageFlags*)submitPipelineStages.Data;
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.pWaitSemaphores = &GetSemaphoresPtr()->PresentComplete;
            submitInfo.signalSemaphoreCount = 1;
            submitInfo.pSignalSemaphores = &GetSemaphoresPtr()->RenderComplete;
         }

         private void CreateWindow()
         {
             NativeWindow = new Sdl2Window(Name, 50, 50, 1280, 720, SDL_WindowFlags.Resizable, threadedProcessing: false);
             NativeWindow.X = 50;
             NativeWindow.Y = 50;
         }
         
         private VkResult CreateInstance(bool enableValidation)
         {
            Settings.Validation = enableValidation;

            VkApplicationInfo appInfo = new VkApplicationInfo()
            {
                sType = VkStructureType.ApplicationInfo,
                apiVersion = new Nsg.Viewer.Internal.Version(1, 0, 0),
                pApplicationName = InternalName,
                pEngineName = InternalName,
            };

            NativeList<IntPtr> instanceExtensions = new NativeList<IntPtr>(2);
            instanceExtensions.Add(Strings.VK_KHR_SURFACE_EXTENSION_NAME);
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                instanceExtensions.Add(Strings.VK_KHR_WIN32_SURFACE_EXTENSION_NAME);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                instanceExtensions.Add(Strings.VK_KHR_XLIB_SURFACE_EXTENSION_NAME);
            }
            else
            {
                throw new PlatformNotSupportedException();
            }

            VkInstanceCreateInfo instanceCreateInfo = VkInstanceCreateInfo.New();
            instanceCreateInfo.pApplicationInfo = &appInfo;

            if (instanceExtensions.Count > 0)
            {
                if (enableValidation)
                {
                    instanceExtensions.Add(Strings.VK_EXT_DEBUG_REPORT_EXTENSION_NAME);
                }
                instanceCreateInfo.enabledExtensionCount = instanceExtensions.Count;
                instanceCreateInfo.ppEnabledExtensionNames = (byte**)instanceExtensions.Data;
            }


            if (enableValidation)
            {
                NativeList<IntPtr> enabledLayerNames = new NativeList<IntPtr>(1);
                enabledLayerNames.Add(Strings.StandardValidationLayeName);
                instanceCreateInfo.enabledLayerCount = enabledLayerNames.Count;
                instanceCreateInfo.ppEnabledLayerNames = (byte**)enabledLayerNames.Data;
            }

            VkInstance instance;
            VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
            Instance = instance;
            return result;
         }
     }
 }