//
// Copyright 2018-2019 Sean Spicer 
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//

using System;
using System.Collections.Generic;

namespace Veldrid.SceneGraph.Viewer
{
    internal class SceneSingleton
    {
        private static readonly Lazy<SceneSingleton> Lazy = new Lazy<SceneSingleton>(() => new SceneSingleton());

        private readonly List<Scene> _sceneCache = new List<Scene>();

        private SceneSingleton()
        {
        }

        public static SceneSingleton Instance => Lazy.Value;

        // TODO use timed lock here
        public void Add(Scene scene)
        {
            _sceneCache.Add(scene);
        }

        public void Remove(Scene scene)
        {
            _sceneCache.Remove(scene);
        }

        public Scene GetScene(INode node)
        {
            foreach (var scene in _sceneCache)
                if (null != scene && scene.SceneData == node)
                    return scene;
            return null;
        }
    }

    public class Scene
    {
        internal Scene()
        {
            SceneSingleton.Instance.Add(this);
        }

        public INode SceneData { get; private set; }

        public static Scene GetScene(INode node)
        {
            return SceneSingleton.Instance.GetScene(node);
        }

        protected static Scene GetOrCreateScene(INode node)
        {
            var scene = GetScene(node);

            if (null != scene) return scene;

            scene = new Scene();
            scene.SetSceneData(node);

            return scene;
        }


        public void SetSceneData(INode node)
        {
            SceneData = node;
        }
    }
}