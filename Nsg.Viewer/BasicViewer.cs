using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
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
            get => InternalName;
            set => InternalName = value;
        }

        public VkInstance Instance { get; protected set; }

        private Sdl2Window NativeWindow { get; set; }

        internal FixedUtf8String title { get; set; } = "NexSceneGraph Basic Viewer";

        private FixedUtf8String InternalName { get; set; } = "BasicViewer";
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
        internal VkCommandPool cmdPool => _cmdPool;
        public VkRenderPass renderPass => _renderPass;
        public VkPipelineCache pipelineCache => _pipelineCache;
        internal NativeList<VkFramebuffer> frameBuffers { get; set; } = new NativeList<VkFramebuffer>();
        internal NativeList<VkCommandBuffer> drawCmdBuffers { get; set; } = new NativeList<VkCommandBuffer>();

        private NativeList<Semaphores> semaphores = new NativeList<Semaphores>(1, 1);
        private Semaphores* GetSemaphoresPtr() => (Semaphores*) semaphores.GetAddress(0);

        internal VkSubmitInfo submitInfo;
        internal NativeList<VkPipelineStageFlags> submitPipelineStages = CreateSubmitPipelineStages();

        private static NativeList<VkPipelineStageFlags> CreateSubmitPipelineStages()
            => new NativeList<VkPipelineStageFlags>() {VkPipelineStageFlags.ColorAttachmentOutput};

        public DepthStencil DepthStencil;

        private VkDescriptorSetLayout _descriptorSetLayout;
        private VkPipelineLayout _pipelineLayout;
        private VkDescriptorSet _descriptorSet;
        private VkPipeline _pipeline;
        public VkDescriptorSetLayout DescriptorSetLayout => _descriptorSetLayout;
        public VkPipelineLayout PipelineLayout => _pipelineLayout;
        public VkDescriptorPool DescriptorPool { get; private set; }
        public VkDescriptorSet DescriptorSet => _descriptorSet;
        public VkPipeline Pipeline => _pipeline;

        protected VkRenderPass _renderPass;
        private VkPipelineCache _pipelineCache;
        private VkCommandPool _cmdPool;
        protected VkDescriptorPool descriptorPool;

        internal VkSemaphore PresentCompleteSemaphore => _presentCompleteSemaphore;
        internal VkSemaphore RenderCompleteSemaphore => _renderCompleteSemaphore;
        internal NativeList<VkFence> waitFences { get; } = new NativeList<VkFence>();
        internal const ulong DEFAULT_FENCE_TIMEOUT = 100000000000;
        private VkSemaphore _presentCompleteSemaphore;
        private VkSemaphore _renderCompleteSemaphore;

        // Window dimensions
        public uint width { get; protected set; } = 1280;
        public uint height { get; protected set; } = 720;

        // Destination dimensions for resizing the window
        private uint destWidth;
        private uint destHeight;
        private bool viewUpdated;
        private int frameCounter;
        protected float frameTimer;
        protected bool paused = false;
        protected bool prepared;

        protected uint currentBuffer;

        float fpsTimer = 0.0f;
        protected bool enableTextOverlay = false;
        private uint lastFPS;
        private readonly FrameTimeAverager _frameTimeAverager = new FrameTimeAverager(666);

        protected float timer = 0.0f;

        // Multiplier for speeding up (or slowing down) the global timer
        protected float timerSpeed = 0.25f;

        public BasicViewer()
        {
            InitVulkan();
            CreateWindow();
        }

        public void Show()
        {
            NativeWindow.Visible = true;
            NativeWindow.Resized += OnNativeWindowResized;

            InitSwapchain();
            Prepare();

            RenderLoop();
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
            IntPtr* physicalDevices = stackalloc IntPtr[(int) gpuCount];
            err = vkEnumeratePhysicalDevices(Instance, &gpuCount, (VkPhysicalDevice*) physicalDevices);
            if (err != VkResult.Success)
            {
                throw new InvalidOperationException("Could not enumerate physical devices.");
            }

            // GPU selection

            // Select physical Device to be used for the Vulkan example
            // Defaults to the first Device unless specified by command line

            uint selectedDevice = 0;
            // TODO: Implement arg parsing, etc.

            PhysicalDevice = ((VkPhysicalDevice*) physicalDevices)[selectedDevice];

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
            Util.CheckResult(
                vkCreateSemaphore(device, &semaphoreCreateInfo, null, &GetSemaphoresPtr()->PresentComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands have been sumbitted and executed
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null,
                &GetSemaphoresPtr()->RenderComplete));
            // Create a semaphore used to synchronize command submission
            // Ensures that the image is not presented until all commands for the text overlay have been sumbitted and executed
            // Will be inserted after the render complete semaphore if the text overlay is enabled
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null,
                &GetSemaphoresPtr()->TextOverlayComplete));

            // Set up submit info structure
            // Semaphores will stay the same during application lifetime
            // Command buffer submission info is set by each example
            submitInfo = Initializers.SubmitInfo();
            submitInfo.pWaitDstStageMask = (VkPipelineStageFlags*) submitPipelineStages.Data;
            submitInfo.waitSemaphoreCount = 1;
            submitInfo.pWaitSemaphores = &GetSemaphoresPtr()->PresentComplete;
            submitInfo.signalSemaphoreCount = 1;
            submitInfo.pSignalSemaphores = &GetSemaphoresPtr()->RenderComplete;
        }

        private void CreateWindow()
        {
            NativeWindow = new Sdl2Window(title, 50, 50, 1280, 720, SDL_WindowFlags.Resizable,
                threadedProcessing: false);
            NativeWindow.X = 50;
            NativeWindow.Y = 50;
        }

        private void OnNativeWindowResized()
        {
            windowResize();
        }

        void windowResize()
        {
            if (!prepared)
            {
                return;
            }

            prepared = false;

            // Ensure all operations on the device have been finished before destroying resources
            vkDeviceWaitIdle(device);

            // Recreate swap chain
            width = destWidth;
            height = destHeight;
            SetupSwapChain();

            // Recreate the frame buffers

            vkDestroyImageView(device, DepthStencil.View, null);
            vkDestroyImage(device, DepthStencil.Image, null);
            vkFreeMemory(device, DepthStencil.Mem, null);
            SetupDepthStencil();

            for (uint i = 0; i < frameBuffers.Count; i++)
            {
                vkDestroyFramebuffer(device, frameBuffers[i], null);
            }

            SetupFrameBuffer();

            // Command buffers need to be recreated as they may store
            // references to the recreated frame buffer
            destroyCommandBuffers();
            CreateCommandBuffers();
            buildCommandBuffers();

            vkDeviceWaitIdle(device);

            if (enableTextOverlay)
            {
                //textOverlay->reallocateCommandBuffers();
                //updateTextOverlay();
            }

            // camera.updateAspectRatio((float)width / (float)height);

            // Notify derived class
            windowResized();
            viewChanged();

            prepared = true;
        }

        private void InitSwapchain()
        {
            Swapchain.InitSurface(NativeWindow.SdlWindowHandle);
        }

        public virtual void Prepare()
        {
            if (vulkanDevice.EnableDebugMarkers)
            {
                // vks::debugmarker::setup(Device);
            }

            CreateCommandPool();
            SetupSwapChain();
            CreateCommandBuffers();
            SetupDepthStencil();
            SetupRenderPass();
            CreatePipelineCache();
            SetupFrameBuffer();

            PrepareSynchronizationPrimitives();

            // TODO Prepare Vertices & Uniforms

            SetupDescriptorSetLayout();

            // TODO Prepare Pipelines

            SetupDescriptorPool();
            SetupDescriptorSet();
            buildCommandBuffers();
            prepared = true;
        }

        private void CreateCommandPool()
        {
            VkCommandPoolCreateInfo cmdPoolInfo = VkCommandPoolCreateInfo.New();
            cmdPoolInfo.queueFamilyIndex = Swapchain.QueueNodeIndex;
            cmdPoolInfo.flags = VkCommandPoolCreateFlags.ResetCommandBuffer;
            Util.CheckResult(vkCreateCommandPool(device, &cmdPoolInfo, null, out _cmdPool));
        }

        private void SetupSwapChain()
        {
            uint width, height;
            Swapchain.Create(&width, &height, Settings.VSync);

            this.width = width;
            this.height = height;
        }

        protected void CreateCommandBuffers()
        {
            // Create one command buffer for each swap chain image and reuse for rendering
            drawCmdBuffers.Resize(Swapchain.ImageCount);
            drawCmdBuffers.Count = Swapchain.ImageCount;

            VkCommandBufferAllocateInfo cmdBufAllocateInfo =
                Initializers.CommandBufferAllocateInfo(cmdPool, VkCommandBufferLevel.Primary, drawCmdBuffers.Count);

            Util.CheckResult(vkAllocateCommandBuffers(device, ref cmdBufAllocateInfo,
                (VkCommandBuffer*) drawCmdBuffers.Data));
        }

        protected virtual void windowResized()
        {
        }

        protected virtual void buildCommandBuffers()
        {
            VkCommandBufferBeginInfo cmdBufInfo = VkCommandBufferBeginInfo.New();

            // Set clear values for all framebuffer attachments with loadOp set to clear
            // We use two attachments (color and depth) that are cleared at the start of the subpass and as such we need to set clear values for both
            byte* clearValuesData = stackalloc byte[2 * sizeof(VkClearValue)];
            VkClearValue* clearValues = (VkClearValue*) clearValuesData;
            clearValues[0].color = new VkClearColorValue(0.2f, 0.0f, 0.2f);
            clearValues[1].depthStencil = new VkClearDepthStencilValue() {depth = 1.0f, stencil = 0};

            VkRenderPassBeginInfo renderPassBeginInfo = VkRenderPassBeginInfo.New();
            renderPassBeginInfo.renderPass = renderPass;
            renderPassBeginInfo.renderArea.offset.x = 0;
            renderPassBeginInfo.renderArea.offset.y = 0;
            renderPassBeginInfo.renderArea.extent.width = width;
            renderPassBeginInfo.renderArea.extent.height = height;
            renderPassBeginInfo.clearValueCount = 2;
            renderPassBeginInfo.pClearValues = clearValues;

            for (int i = 0; i < drawCmdBuffers.Count; ++i)
            {
                // Set target frame buffer
                renderPassBeginInfo.framebuffer = frameBuffers[i];

                Util.CheckResult(vkBeginCommandBuffer(drawCmdBuffers[i], ref cmdBufInfo));

                // Start the first sub pass specified in our default render pass setup by the base class
                // This will clear the color and depth attachment
                vkCmdBeginRenderPass(drawCmdBuffers[i], ref renderPassBeginInfo, VkSubpassContents.Inline);

                // Update dynamic viewport state
                VkViewport viewport = new VkViewport();
                viewport.height = (float) height;
                viewport.width = (float) width;
                viewport.minDepth = (float) 0.0f;
                viewport.maxDepth = (float) 1.0f;
                vkCmdSetViewport(drawCmdBuffers[i], 0, 1, &viewport);

                // Update dynamic scissor state
                VkRect2D scissor = new VkRect2D();
                scissor.extent.width = width;
                scissor.extent.height = height;
                scissor.offset.x = 0;
                scissor.offset.y = 0;
                vkCmdSetScissor(drawCmdBuffers[i], 0, 1, &scissor);

                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~ 
                // TODO - This is where Scenegraph data gets drawn?
                //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

                vkCmdEndRenderPass(drawCmdBuffers[i]);

                // Ending the render pass will add an implicit barrier transitioning the frame buffer color attachment to 
                // VK_IMAGE_LAYOUT_PRESENT_SRC_KHR for presenting it to the windowing system

                Util.CheckResult(vkEndCommandBuffer(drawCmdBuffers[i]));
            }
        }

        protected virtual void viewChanged()
        {
        }

        protected void destroyCommandBuffers()
        {
            vkFreeCommandBuffers(device, cmdPool, drawCmdBuffers.Count, drawCmdBuffers.Data);
        }

        protected virtual void SetupDepthStencil()
        {
            VkImageCreateInfo image = VkImageCreateInfo.New();
            image.imageType = VkImageType.Image2D;
            image.format = DepthFormat;
            image.extent = new VkExtent3D() {width = width, height = height, depth = 1};
            image.mipLevels = 1;
            image.arrayLayers = 1;
            image.samples = VkSampleCountFlags.Count1;
            image.tiling = VkImageTiling.Optimal;
            image.usage = (VkImageUsageFlags.DepthStencilAttachment | VkImageUsageFlags.TransferSrc);
            image.flags = 0;

            VkMemoryAllocateInfo mem_alloc = VkMemoryAllocateInfo.New();
            mem_alloc.allocationSize = 0;
            mem_alloc.memoryTypeIndex = 0;

            VkImageViewCreateInfo depthStencilView = VkImageViewCreateInfo.New();
            depthStencilView.viewType = VkImageViewType.Image2D;
            depthStencilView.format = DepthFormat;
            depthStencilView.flags = 0;
            depthStencilView.subresourceRange = new VkImageSubresourceRange();
            depthStencilView.subresourceRange.aspectMask = (VkImageAspectFlags.Depth | VkImageAspectFlags.Stencil);
            depthStencilView.subresourceRange.baseMipLevel = 0;
            depthStencilView.subresourceRange.levelCount = 1;
            depthStencilView.subresourceRange.baseArrayLayer = 0;
            depthStencilView.subresourceRange.layerCount = 1;

            Util.CheckResult(vkCreateImage(device, &image, null, out DepthStencil.Image));
            vkGetImageMemoryRequirements(device, DepthStencil.Image, out VkMemoryRequirements memReqs);
            mem_alloc.allocationSize = memReqs.size;
            mem_alloc.memoryTypeIndex =
                vulkanDevice.getMemoryType(memReqs.memoryTypeBits, VkMemoryPropertyFlags.DeviceLocal);
            Util.CheckResult(vkAllocateMemory(device, &mem_alloc, null, out DepthStencil.Mem));
            Util.CheckResult(vkBindImageMemory(device, DepthStencil.Image, DepthStencil.Mem, 0));

            depthStencilView.image = DepthStencil.Image;
            Util.CheckResult(vkCreateImageView(device, ref depthStencilView, null, out DepthStencil.View));
        }

        protected virtual void SetupRenderPass()
        {
            using (NativeList<VkAttachmentDescription> attachments = new NativeList<VkAttachmentDescription>())
            {
                attachments.Count = 2;
                // Color attachment
                attachments[0] = new VkAttachmentDescription();
                attachments[0].format = Swapchain.ColorFormat;
                attachments[0].samples = VkSampleCountFlags.Count1;
                attachments[0].loadOp = VkAttachmentLoadOp.Clear;
                attachments[0].storeOp = VkAttachmentStoreOp.Store;
                attachments[0].stencilLoadOp = VkAttachmentLoadOp.DontCare;
                attachments[0].stencilStoreOp = VkAttachmentStoreOp.DontCare;
                attachments[0].initialLayout = VkImageLayout.Undefined;
                attachments[0].finalLayout = VkImageLayout.PresentSrcKHR;
                // Depth attachment
                attachments[1] = new VkAttachmentDescription();
                attachments[1].format = DepthFormat;
                attachments[1].samples = VkSampleCountFlags.Count1;
                attachments[1].loadOp = VkAttachmentLoadOp.Clear;
                attachments[1].storeOp = VkAttachmentStoreOp.Store;
                attachments[1].stencilLoadOp = VkAttachmentLoadOp.DontCare;
                attachments[1].stencilStoreOp = VkAttachmentStoreOp.DontCare;
                attachments[1].initialLayout = VkImageLayout.Undefined;
                attachments[1].finalLayout = VkImageLayout.DepthStencilAttachmentOptimal;

                VkAttachmentReference colorReference = new VkAttachmentReference();
                colorReference.attachment = 0;
                colorReference.layout = VkImageLayout.ColorAttachmentOptimal;

                VkAttachmentReference depthReference = new VkAttachmentReference();
                depthReference.attachment = 1;
                depthReference.layout = VkImageLayout.DepthStencilAttachmentOptimal;

                VkSubpassDescription subpassDescription = new VkSubpassDescription();
                subpassDescription.pipelineBindPoint = VkPipelineBindPoint.Graphics;
                subpassDescription.colorAttachmentCount = 1;
                subpassDescription.pColorAttachments = &colorReference;
                subpassDescription.pDepthStencilAttachment = &depthReference;
                subpassDescription.inputAttachmentCount = 0;
                subpassDescription.pInputAttachments = null;
                subpassDescription.preserveAttachmentCount = 0;
                subpassDescription.pPreserveAttachments = null;
                subpassDescription.pResolveAttachments = null;

                // Subpass dependencies for layout transitions
                using (NativeList<VkSubpassDependency> dependencies = new NativeList<VkSubpassDependency>(2))
                {
                    dependencies.Count = 2;

                    dependencies[0].srcSubpass = SubpassExternal;
                    dependencies[0].dstSubpass = 0;
                    dependencies[0].srcStageMask = VkPipelineStageFlags.BottomOfPipe;
                    dependencies[0].dstStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
                    dependencies[0].srcAccessMask = VkAccessFlags.MemoryRead;
                    dependencies[0].dstAccessMask =
                        (VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite);
                    dependencies[0].dependencyFlags = VkDependencyFlags.ByRegion;

                    dependencies[1].srcSubpass = 0;
                    dependencies[1].dstSubpass = SubpassExternal;
                    dependencies[1].srcStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
                    dependencies[1].dstStageMask = VkPipelineStageFlags.BottomOfPipe;
                    dependencies[1].srcAccessMask =
                        (VkAccessFlags.ColorAttachmentRead | VkAccessFlags.ColorAttachmentWrite);
                    dependencies[1].dstAccessMask = VkAccessFlags.MemoryRead;
                    dependencies[1].dependencyFlags = VkDependencyFlags.ByRegion;

                    VkRenderPassCreateInfo renderPassInfo = new VkRenderPassCreateInfo();
                    renderPassInfo.sType = VkStructureType.RenderPassCreateInfo;
                    renderPassInfo.attachmentCount = attachments.Count;
                    renderPassInfo.pAttachments = (VkAttachmentDescription*) attachments.Data.ToPointer();
                    renderPassInfo.subpassCount = 1;
                    renderPassInfo.pSubpasses = &subpassDescription;
                    renderPassInfo.dependencyCount = dependencies.Count;
                    renderPassInfo.pDependencies = (VkSubpassDependency*) dependencies.Data;

                    Util.CheckResult(vkCreateRenderPass(device, &renderPassInfo, null, out _renderPass));
                }
            }
        }

        private void CreatePipelineCache()
        {
            VkPipelineCacheCreateInfo pipelineCacheCreateInfo = VkPipelineCacheCreateInfo.New();
            Util.CheckResult(vkCreatePipelineCache(device, ref pipelineCacheCreateInfo, null, out _pipelineCache));
        }

        // Create the Vulkan synchronization primitives used in this example
        private void PrepareSynchronizationPrimitives()
        {
            // Semaphores (Used for correct command ordering)
            VkSemaphoreCreateInfo semaphoreCreateInfo = VkSemaphoreCreateInfo.New();

            // Semaphore used to ensures that image presentation is complete before starting to submit again
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, out _presentCompleteSemaphore));

            // Semaphore used to ensures that all commands submitted have been finished before submitting the image to the queue
            Util.CheckResult(vkCreateSemaphore(device, &semaphoreCreateInfo, null, out _renderCompleteSemaphore));

            // Fences (Used to check draw command buffer completion)
            VkFenceCreateInfo fenceCreateInfo = VkFenceCreateInfo.New();
            // Create in signaled state so we don't wait on first render of each command buffer
            fenceCreateInfo.flags = VkFenceCreateFlags.Signaled;
            waitFences.Resize(drawCmdBuffers.Count);
            waitFences.Count = drawCmdBuffers.Count;
            for (uint i = 0; i < waitFences.Count; i++)
            {
                Util.CheckResult(vkCreateFence(device, ref fenceCreateInfo, null, (VkFence*) waitFences.GetAddress(i)));
            }
        }

        protected virtual void SetupFrameBuffer()
        {
            using (NativeList<VkImageView> attachments = new NativeList<VkImageView>(2))
            {
                attachments.Count = 2;
                // Depth/Stencil attachment is the same for all frame buffers
                attachments[1] = DepthStencil.View;

                VkFramebufferCreateInfo frameBufferCreateInfo = VkFramebufferCreateInfo.New();
                frameBufferCreateInfo.renderPass = renderPass;
                frameBufferCreateInfo.attachmentCount = 2;
                frameBufferCreateInfo.pAttachments = (VkImageView*) attachments.Data;
                frameBufferCreateInfo.width = width;
                frameBufferCreateInfo.height = height;
                frameBufferCreateInfo.layers = 1;

                // Create frame buffers for every swap chain image
                frameBuffers.Count = (Swapchain.ImageCount);
                for (uint i = 0; i < frameBuffers.Count; i++)
                {
                    attachments[0] = Swapchain.Buffers[i].View;
                    Util.CheckResult(vkCreateFramebuffer(device, ref frameBufferCreateInfo, null,
                        (VkFramebuffer*) Unsafe.AsPointer(ref frameBuffers[i])));
                }
            }
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
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                instanceExtensions.Add(Strings.VK_MVK_MACOS_SURFACE_EXTENSION_NAME);
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
                instanceCreateInfo.ppEnabledExtensionNames = (byte**) instanceExtensions.Data;
            }


            if (enableValidation)
            {
                NativeList<IntPtr> enabledLayerNames = new NativeList<IntPtr>(1);
                enabledLayerNames.Add(Strings.StandardValidationLayerName);
                instanceCreateInfo.enabledLayerCount = enabledLayerNames.Count;
                instanceCreateInfo.ppEnabledLayerNames = (byte**) enabledLayerNames.Data;
            }

            VkInstance instance;
            VkResult result = vkCreateInstance(&instanceCreateInfo, null, &instance);
            Instance = instance;
            return result;
        }

        void SetupDescriptorPool()
        {
            // We need to tell the API the number of max. requested descriptors per type
            VkDescriptorPoolSize typeCount;
            // This example only uses one descriptor type (uniform buffer) and only requests one descriptor of this type
            typeCount.type = VkDescriptorType.UniformBuffer;
            typeCount.descriptorCount = 1;
            // For additional types you need to add new entries in the type count list
            // E.g. for two combined image samplers :
            // typeCounts[1].type = VK_DESCRIPTOR_TYPE_COMBINED_IMAGE_SAMPLER;
            // typeCounts[1].descriptorCount = 2;

            // Create the global descriptor pool
            // All descriptors used in this example are allocated from this pool
            VkDescriptorPoolCreateInfo descriptorPoolInfo = VkDescriptorPoolCreateInfo.New();
            descriptorPoolInfo.poolSizeCount = 1;
            descriptorPoolInfo.pPoolSizes = &typeCount;
            // Set the max. number of descriptor sets that can be requested from this pool (requesting beyond this limit will result in an error)
            descriptorPoolInfo.maxSets = 1;

            VkDescriptorPool descriptorPool;
            Util.CheckResult(vkCreateDescriptorPool(device, ref descriptorPoolInfo, null, out descriptorPool));
            DescriptorPool = descriptorPool;
        }

        void SetupDescriptorSetLayout()
        {
            // Setup layout of descriptors used in this example
            // Basically connects the different shader stages to descriptors for binding uniform buffers, image samplers, etc.
            // So every shader binding should map to one descriptor set layout binding

            // Binding 0: Uniform buffer (Vertex shader)
            VkDescriptorSetLayoutBinding layoutBinding = new VkDescriptorSetLayoutBinding();
            layoutBinding.descriptorType = VkDescriptorType.UniformBuffer;
            layoutBinding.descriptorCount = 1;
            layoutBinding.stageFlags = VkShaderStageFlags.Vertex;
            layoutBinding.pImmutableSamplers = null;

            VkDescriptorSetLayoutCreateInfo descriptorLayout = VkDescriptorSetLayoutCreateInfo.New();
            descriptorLayout.bindingCount = 1;
            descriptorLayout.pBindings = &layoutBinding;

            Util.CheckResult(vkCreateDescriptorSetLayout(device, ref descriptorLayout, null, out _descriptorSetLayout));

            // Create the Pipeline layout that is used to generate the rendering pipelines that are based on this descriptor set layout
            // In a more complex scenario you would have different Pipeline layouts for different descriptor set layouts that could be reused
            VkPipelineLayoutCreateInfo pPipelineLayoutCreateInfo = new VkPipelineLayoutCreateInfo();
            pPipelineLayoutCreateInfo.sType = VkStructureType.PipelineLayoutCreateInfo;
            pPipelineLayoutCreateInfo.pNext = null;
            pPipelineLayoutCreateInfo.setLayoutCount = 1;
            VkDescriptorSetLayout dsl = _descriptorSetLayout;
            pPipelineLayoutCreateInfo.pSetLayouts = &dsl;

            Util.CheckResult(vkCreatePipelineLayout(device, &pPipelineLayoutCreateInfo, null, out _pipelineLayout));
        }

        void SetupDescriptorSet()
        {
        }

        protected virtual void render()
        {
            if (prepared)
            {
                Draw();
            }
        }

        void Draw()
        {
            // Get next image in the swap chain (back/front buffer)
            Util.CheckResult(Swapchain.AcquireNextImage(PresentCompleteSemaphore, ref currentBuffer));

            // Use a fence to wait until the command buffer has finished execution before using it again
            Util.CheckResult(vkWaitForFences(device, 1, ref waitFences[currentBuffer], True, ulong.MaxValue));
            Util.CheckResult(vkResetFences(device, 1, ref waitFences[currentBuffer]));

            // Pipeline stage at which the queue submission will wait (via pWaitSemaphores)
            VkPipelineStageFlags waitStageMask = VkPipelineStageFlags.ColorAttachmentOutput;
            // The submit info structure specifices a command buffer queue submission batch
            VkSubmitInfo submitInfo = VkSubmitInfo.New();
            submitInfo.pWaitDstStageMask =
                &waitStageMask; // Pointer to the list of Pipeline stages that the semaphore waits will occur at
            var pcs = PresentCompleteSemaphore;
            submitInfo.pWaitSemaphores =
                &pcs; // Semaphore(s) to wait upon before the submitted command buffer starts executing
            submitInfo.waitSemaphoreCount = 1; // One wait semaphore
            var rcs = RenderCompleteSemaphore;
            submitInfo.pSignalSemaphores = &rcs; // Semaphore(s) to be signaled when command buffers have completed
            submitInfo.signalSemaphoreCount = 1; // One signal semaphore
            submitInfo.pCommandBuffers =
                (VkCommandBuffer*) drawCmdBuffers
                    .GetAddress(currentBuffer); // Command buffers(s) to execute in this batch (submission)
            submitInfo.commandBufferCount = 1; // One command buffer

            // Submit to the graphics queue passing a wait fence
            Util.CheckResult(vkQueueSubmit(queue, 1, ref submitInfo, waitFences[currentBuffer]));

            // Present the current buffer to the swap chain
            // Pass the semaphore signaled by the command buffer submission from the submit info as the wait semaphore for swap chain presentation
            // This ensures that the image is not presented to the windowing system until all commands have been submitted
            Util.CheckResult(Swapchain.QueuePresent(queue, currentBuffer, RenderCompleteSemaphore));
        }

        private string getWindowTitle()
        {
            var dp = DeviceProperties;
            string device = Encoding.UTF8.GetString(dp.deviceName, (int) MaxPhysicalDeviceNameSize);
            int firstNull = device.IndexOf('\0');
            device = device.Remove(firstNull);
            string windowTitle;
            windowTitle = title + " - " + device;
            if (!enableTextOverlay)
            {
                windowTitle += " - " + _frameTimeAverager.CurrentAverageFramesPerSecond.ToString("000.0 fps / ") +
                               _frameTimeAverager.CurrentAverageFrameTime.ToString("#00.00 ms");
            }

            return windowTitle;
        }

        public void RenderLoop()
        {
            destWidth = width;
            destHeight = height;
            while (NativeWindow.Exists)
            {
                var tStart = DateTime.Now;
                if (viewUpdated)
                {
                    viewUpdated = false;
                    viewChanged();
                }

                NativeWindow.PumpEvents();

                if (!NativeWindow.Exists)
                {
                    // Exit early if the window was closed this frame.
                    break;
                }

                render();
                frameCounter++;
                var tEnd = DateTime.Now;
                var tDiff = tEnd - tStart;
                frameTimer = (float) tDiff.TotalMilliseconds / 1000.0f;
                _frameTimeAverager.AddTime(tDiff.TotalMilliseconds);
                /*
                camera.update(frameTimer);
                if (camera.moving())
                {
                    viewUpdated = true;
                }
                */
                // Convert to clamped timer value
                if (!paused)
                {
                    timer += timerSpeed * frameTimer;
                    if (timer > 1.0)
                    {
                        timer -= 1.0f;
                    }
                }

                fpsTimer += (float) tDiff.TotalMilliseconds * 1000f;
                if (fpsTimer > 1000.0f)
                {
                    if (!enableTextOverlay)
                    {
                        NativeWindow.Title = getWindowTitle();
                    }

                    lastFPS = (uint) (1.0f / frameTimer);
                    // updateTextOverlay();
                    fpsTimer = 0.0f;
                    frameCounter = 0;
                }
            }

            // Flush device to make sure all resources can be freed
            vkDeviceWaitIdle(device);
        }
    }
}