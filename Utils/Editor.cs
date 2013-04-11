using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Phantom.Core;
using Phantom.Graphics;
using Phantom.Shapes;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Diagnostics;
using Phantom;
using Phantom.Misc;
using Microsoft.Xna.Framework.Graphics;
using Phantom.UI;
using System.Globalization;

namespace Phantom.Utils
{
    public class Editor : Component
    {
        public class EditorState : GameState
        {
            private Renderer renderer;
            public Editor Editor;
            public EditorState(GameState editing)
            {
                this.Propagate = false;
                this.Transparent = true;
                renderer = new Renderer(1, Renderer.ViewportPolicy.Fit, Renderer.RenderOptions.Canvas);
                Editor = new Editor(editing);
                AddComponent(Editor);
                AddComponent(renderer);
            }
        }

        private enum LayerType { Entities, Tiles, Properties }

        private struct EditorLayer
        {
            public Component Layer;
            public string Name;
            public LayerType Type;
            public Dictionary<string, PCNComponent> EntityList;
            public int TileSize;
            public EditorLayer(Component layer, string name, LayerType type, Dictionary<string, PCNComponent> entityList, int tileSize)
            {
                this.Layer = layer;
                this.Name = name;
                this.Type = type;
                this.EntityList = entityList;
                this.TileSize = tileSize;
            }
        }

        private static SpriteFont font;

        public GameState Editing;
        private List<EditorLayer> layers;
        private int currentLayer = -1;

        private Entity[] tileMap;
        private int tilesX;
        private int tilesY;

        private Entity drawingEntity;
        private PCNComponent drawingType;
        private Entity hoveringEntity;
        private Entity selectedEntity;

        private MouseState previousMouse = Mouse.GetState();
        private Vector2 mousePosition;
        private Vector2 mouseOffset;
        private KeyboardState previousKeyboard = Keyboard.GetState();

        private PhWindow layerWindow;
        private PhWindow propertiesWindow;
        private PhWindow entitiesWindow;
        private PhWindow main;
        private PhControl windows;
        private Component propertiesWindowTarget;


        public static void Initialize(SpriteFont font)
        {
            MapLoader.Initialize();
            GUISettings.Initialize(font);
            PhantomGame.Game.Console.Register("editor", "Opens editor window.", delegate(string[] argv)
            {
                if (!(PhantomGame.Game.CurrentState is EditorState))
                    PhantomGame.Game.PushState(new EditorState(PhantomGame.Game.CurrentState));
                else
                {
                    PhantomGame.Game.PopState();
                    PhantomGame.Game.CurrentState.HandleMessage(Messages.MapLoaded, null);
                }
            });

            Editor.font = font;
        }


        public Editor(GameState editing)
        {
            this.Editing = editing;
            this.Editing.HandleMessage(Messages.MapReset, null);
            if (Editing.Camera != null)
                this.Editing.Camera.HandleMessage(Messages.CameraStopFollowing, null);

            layers = new List<EditorLayer>();
            foreach (Component c in editing.Components)
            {
                if (c.Properties != null && c.Properties.GetString("editable", null) != null)
                {
                    string name = c.Properties.GetString("editable", null);
                    layers.Add(new EditorLayer(c, name + " (Properties)", LayerType.Properties, null, 0));
                    if (c is EntityLayer)
                    {
                        if (MapLoader.EntityLists.ContainsKey(c.Properties.GetString("tileList", "")))
                            layers.Add(new EditorLayer(c, name + " (Tiles)", LayerType.Tiles, MapLoader.EntityLists[c.Properties.GetString("tileList", "")], c.Properties.GetInt("tileSize", 0)));
                        if (MapLoader.EntityLists.ContainsKey(c.Properties.GetString("entityList", "")))
                            layers.Add(new EditorLayer(c, name + " (Entities)", LayerType.Entities, MapLoader.EntityLists[c.Properties.GetString("entityList", "")], 0));
                    }
                }
                
            }

            if (layers.Count > 0)
            {
                currentLayer = 0;
                for (int i = 0; i < layers.Count; i++)
                {
                    if (layers[i].Type != LayerType.Properties)
                    {
                        currentLayer = i;
                        break;
                    }
                }
            }
            ChangeLayer();

            CreateWindows();
        }

        private void CreateWindows()
        {
            windows = new PhControl(0, 0, PhantomGame.Game.Resolution.Width, PhantomGame.Game.Resolution.Height);
            windows.Ghost = true;
            AddComponent(windows);

            main = new PhWindow(100, 100, 500, 500, "Main Menu");
            main.Ghost = true;
            int y = 30;
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Layers", StartSelectLayer));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Entities", StartSelectEntity));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Clear Map", ClearMap));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Save Map", SaveMap));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Open Map", OpenMap));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Close Menu", CloseMenu));
            main.AddComponent(new PhButton(10, y += 32, 480, 24, "Close Editor", CloseEditor));
            windows.AddComponent(main);

            layerWindow = new PhWindow(100, 100, 500, 500, "Layers");
            layerWindow.Ghost = true;
            for (int i = 0; i < layers.Count; i++)
                layerWindow.AddComponent(new PhButton(10, 30+i*32, 480, 24, layers[i].Name, SelectLayer));
            windows.AddComponent(layerWindow);
        }

        private PhWindow BuildEntitiesWindow(Dictionary<string, PCNComponent> entityList, bool includeNone, string name)
        {
            entitiesWindow = new PhWindow(100, 100, 500, 500, name);
            entitiesWindow.Ghost = true;
            int x = 10;
            int y = 30;
            if (includeNone)
            {
                entitiesWindow.AddComponent(new PhButton(x, y, 160, 24, "<none>", SelectEntity));
                y += 32;
            }

            foreach (KeyValuePair<string, PCNComponent> entity in entityList)
            {
                entitiesWindow.AddComponent(new PhButton(x, y, 160, 24, entity.Key, SelectEntity));
                y += 32;
                if (y > 500 - 32)
                {
                    x += 168;
                    y = 30;
                }
            }

            entitiesWindow.OnClose = CloseEntityWindow;
            return entitiesWindow;
        }

        private void CloseEntityWindow(PhControl sender)
        {
            if (entitiesWindow != null)
            {
                windows.RemoveComponent(entitiesWindow);
                entitiesWindow = null;
            }
        }

        private void StartSelectEntity(PhControl sender)
        {
            if (layers[currentLayer].Type == LayerType.Tiles)
            {
                PhWindow window = BuildEntitiesWindow(layers[currentLayer].EntityList, true, "Tiles");
                windows.AddComponent(window);
                window.Show();
            }
            if (layers[currentLayer].Type == LayerType.Entities)
            {
                PhWindow window = BuildEntitiesWindow(layers[currentLayer].EntityList, false, "Entities");
                windows.AddComponent(window);
                window.Show();
            }
        }

        private void StartSelectLayer(PhControl sender)
        {
            layerWindow.Show();
        }

        private void ClearMap(PhControl sender)
        {
            for (int i = 0; i < layers.Count; i++)
            {
                if (layers[i].Layer is EntityLayer && layers[i].Type == LayerType.Properties)
                {
                    ((EntityLayer)layers[i].Layer).ClearComponents();
                }
            }
            main.Hide();
        }

        private void SaveMap(PhControl sender)
        {
            string filename = "";
            if (Editing.Properties != null)
                filename = Editing.Properties.GetString("filename", "");
            windows.AddComponent(new PhInputDialog(100, 100, "Save Map", "Filename:", filename, ConfirmSave));
        }

        private void ConfirmSave(PhControl sender)
        {
            MapLoader.SaveMap((sender as PhInputDialog).Result);
        }
        

        private void OpenMap(PhControl sender)
        {
            string filename = "";
            if (Editing.Properties != null)
                filename = Editing.Properties.GetString("filename", "");
            windows.AddComponent(new PhInputDialog(100, 100, "Open Map", "Filename:", filename, ConfirmOpen));
        }

        private void ConfirmOpen(PhControl sender)
        {
            MapLoader.OpenMap((sender as PhInputDialog).Result);
            if (Editing.Camera != null)
                this.Editing.Camera.HandleMessage(Messages.CameraStopFollowing, null);
        }

        private void CloseMenu(PhControl sender)
        {
            main.Hide();
        }


        private void SelectEntity(PhControl sender)
        {
            PhButton button = sender as PhButton;
            if (layers[currentLayer].EntityList != null)
            {
                drawingType = null;
                drawingEntity = null;
                if (layers[currentLayer].EntityList.ContainsKey(button.Text))
                {
                    drawingType = layers[currentLayer].EntityList[button.Text];
                    drawingEntity = EntityFactory.AssembleEntity(drawingType, button.Text);
                    drawingEntity.Position = mousePosition;
                }
            }
            (button.Parent as PhWindow).Hide();
        }

        private void SelectLayer(PhControl sender)
        {
            layerWindow.Hide();
            PhButton button = sender as PhButton;
            if (button != null)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    if (button.Text == layers[i].Name)
                    {
                        switch (layers[i].Type)
                        {
                            case LayerType.Entities:
                            case LayerType.Tiles:
                                currentLayer = i;
                                ChangeLayer();
                                break;
                            case LayerType.Properties:
                                selectedEntity = null;
                                CreatePropertiesWindow(layers[i].Name, layers[i].Layer);
                                break;
                        }
                        break;
                    }
                }
            }
        }

        private void ChangeLayer()
        {
            hoveringEntity = null;
            selectedEntity = null;
            drawingType = null;
            drawingEntity = null;
            if (currentLayer < 0)
                return;

            if (layers[currentLayer].Type == LayerType.Tiles)
            {
                tilesX = (int)Math.Ceiling(((EntityLayer)layers[currentLayer].Layer).Bounds.X / layers[currentLayer].TileSize);
                tilesY = (int)Math.Ceiling(((EntityLayer)layers[currentLayer].Layer).Bounds.Y / layers[currentLayer].TileSize);
                tileMap = MapLoader.ConstructTileMap((EntityLayer)layers[currentLayer].Layer);
            }
        }


        public override void Update(float elapsed)
        {
            base.Update(elapsed);

            MouseState currentMouse = Mouse.GetState();

            if (windows.DoMouseMove(currentMouse.X, currentMouse.Y)!=null)
            {
                if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                    windows.DoMouseDown();
                if (currentMouse.LeftButton == ButtonState.Released && previousMouse.LeftButton == ButtonState.Pressed)
                    windows.DoMouseUp();
            }
            else
            {
                mousePosition.X = currentMouse.X + Editing.Camera.Left;
                mousePosition.Y = currentMouse.Y + Editing.Camera.Top;

                switch (layers[currentLayer].Type)
                {
                    case LayerType.Tiles:
                        if (drawingEntity != null)
                            drawingEntity.Position = SnapPosition(mousePosition);
                        if (currentMouse.LeftButton == ButtonState.Pressed)
                            MouseLeftDownTiles();
                        if (currentMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released)
                            MouseRightDownTiles();
                        break;
                    case LayerType.Entities:
                        if (drawingEntity != null)
                            drawingEntity.Position = mousePosition;
                        if (currentMouse.LeftButton == ButtonState.Pressed && previousMouse.LeftButton == ButtonState.Released)
                            MouseLeftDownEntities();
                        MouseMoveEntities(currentMouse.LeftButton == ButtonState.Pressed);
                        if (currentMouse.RightButton == ButtonState.Pressed && previousMouse.RightButton == ButtonState.Released)
                            MouseRightDownEntities();
                        int delta = currentMouse.ScrollWheelValue - previousMouse.ScrollWheelValue;
                        if (previousKeyboard.IsKeyDown(Keys.LeftShift) || previousKeyboard.IsKeyDown(Keys.RightShift))
                            delta *= 10;
                        MouseWheelEntities(delta);

                        break;
                }
            }

            previousMouse = currentMouse;

            KeyboardState currentKeyboard = Keyboard.GetState();
            if (windows.Ghost && !(windows.Focus is PhTextEdit))
            {

                if (Editing.Camera != null)
                {
                    Editing.Camera.Update(elapsed);
                    Vector2 camMove = new Vector2();
                    if (currentKeyboard.IsKeyDown(Keys.Left))
                        camMove.X -= elapsed * 500;
                    if (currentKeyboard.IsKeyDown(Keys.Right))
                        camMove.X += elapsed * 500;
                    if (currentKeyboard.IsKeyDown(Keys.Up))
                        camMove.Y -= elapsed * 500;
                    if (currentKeyboard.IsKeyDown(Keys.Down))
                        camMove.Y += elapsed * 500;

                    if (camMove.LengthSquared() > 0)
                        Editing.Camera.HandleMessage(Messages.CameraMoveBy, camMove);
                }

                if ((currentKeyboard.IsKeyDown(Keys.Delete) && previousKeyboard.IsKeyUp(Keys.Delete)) || (currentKeyboard.IsKeyDown(Keys.Back) && previousKeyboard.IsKeyUp(Keys.Back)))
                {
                    if (selectedEntity != null)
                    {
                        selectedEntity.Destroyed = true;
                        layers[currentLayer].Layer.RemoveComponent(selectedEntity);
                        selectedEntity = null;
                    }
                }
                if (currentKeyboard.IsKeyDown(Keys.L) && previousKeyboard.IsKeyUp(Keys.L))
                {
                    if (layerWindow.Ghost)
                        layerWindow.Show();
                    else 
                        layerWindow.Hide();
                }
                if ((currentKeyboard.IsKeyDown(Keys.E) && previousKeyboard.IsKeyUp(Keys.E)))
                {
                    if (entitiesWindow == null)
                        StartSelectEntity(null);
                    else
                        entitiesWindow.Hide();
                }
                if (currentKeyboard.IsKeyDown(Keys.Escape) && previousKeyboard.IsKeyUp(Keys.Escape))
                {
                    CloseEditor(null);
                }
                if (currentKeyboard.IsKeyDown(Keys.Space) && previousKeyboard.IsKeyUp(Keys.Space))
                {
                    main.Show();
                }
            }

            previousKeyboard = currentKeyboard;


        }

        

        private void CloseEditor(PhControl sender)
        {
            Editing.HandleMessage(Messages.MapLoaded, null);
            PhantomGame.Game.PopState();
        }

        private Vector2 SnapPosition(Vector2 position)
        {
            int ts = layers[currentLayer].TileSize;
            if (ts > 0)
            {
                position.X = (float)Math.Floor(position.X / ts) * ts + ts * 0.5f;
                position.Y = (float)Math.Floor(position.Y / ts) * ts + ts * 0.5f;
            }
            return position;
        }

        private void MouseMoveEntities(bool mouseDown)
        {
            EntityLayer entities = layers[currentLayer].Layer as EntityLayer;
            if (!mouseDown)
            {
                List<Entity> all = entities.GetEntitiesAt(mousePosition);
                hoveringEntity = null;
                for (int i = all.Count - 1; i >= 0; i--)
                {
                    if (!all[i].Destroyed && all[i].Properties.GetInt("isTile", 0) == 0)
                    {
                        hoveringEntity = all[i];
                        break;
                    }
                }
            }
            else
            {
                if (selectedEntity != null)
                {
                    selectedEntity.Position = mousePosition + mouseOffset;
                    selectedEntity.Integrate(0);
                }
            }
        }

        private void MouseLeftDownEntities()
        {
            if (hoveringEntity != null) //Selecting
            {
                mouseOffset = hoveringEntity.Position - mousePosition;
                selectedEntity = hoveringEntity;
                string blueprintName = selectedEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, "");
                drawingType = layers[currentLayer].EntityList[blueprintName];
                drawingEntity = EntityFactory.AssembleEntity(drawingType, blueprintName);
                drawingEntity.Position = mousePosition;
                CopyProperties(selectedEntity, drawingEntity);
                //Randomize the seed if there is any
                if (drawingEntity.Properties.GetInt("Seed", -1) > 0)
                {
                    drawingEntity.Properties.Ints["Seed"] = PhantomGame.Randy.Next(int.MaxValue);
                    drawingEntity.HandleMessage(Messages.PropertiesChanged, null);
                }
            }
            else if (drawingType != null) //Drawing
            {
                EntityLayer entities = layers[currentLayer].Layer as EntityLayer;
                string blueprintName = drawingEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, "");
                selectedEntity = EntityFactory.AssembleEntity(drawingType, blueprintName);
                selectedEntity.Position = mousePosition;
                entities.AddComponent(selectedEntity);
                hoveringEntity = selectedEntity;
                CopyProperties(drawingEntity, selectedEntity);
                mouseOffset *= 0;
                //Randomize the seed if there is any
                if (drawingEntity.Properties.GetInt("Seed", -1) > 0)
                {
                    drawingEntity.Properties.Ints["Seed"] = PhantomGame.Randy.Next(int.MaxValue);
                    drawingEntity.HandleMessage(Messages.PropertiesChanged, null);
                }

            }
        }

        private void CopyProperties(Entity source, Entity target)
        {
            target.Orientation = source.Orientation;
            foreach (KeyValuePair<string, int> property in source.Properties.Ints)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                    target.Properties.Ints[property.Key] = property.Value;
            }
            foreach (KeyValuePair<string, float> property in source.Properties.Floats)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                    target.Properties.Floats[property.Key] = property.Value;
            }
            foreach (KeyValuePair<string, object> property in source.Properties.Objects)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                    target.Properties.Objects[property.Key] = property.Value;
            }
            target.HandleMessage(Messages.PropertiesChanged, null);
        }

        private void MouseRightDownEntities()
        {
            if (selectedEntity!=null) {
                CreatePropertiesWindow(selectedEntity.GetType().Name, selectedEntity);
            }
        }

        private void MouseWheelEntities(int delta)
        {
            if (hoveringEntity != null && hoveringEntity == selectedEntity && delta != 0)
            {
                selectedEntity.Orientation = (selectedEntity.Orientation + MathHelper.Pi * 2 - MathHelper.ToRadians(delta / 120)) % (MathHelper.Pi * 2);

            } 
            else if (drawingEntity != null && delta!=0)
            {
                drawingEntity.Orientation = (drawingEntity.Orientation + MathHelper.Pi * 2 - MathHelper.ToRadians(delta/120)) % (MathHelper.Pi * 2);
            }
        }

        private void CreatePropertiesWindow(string name, Component component)
        {
            propertiesWindowTarget = component;
            propertiesWindow = new PhWindow(100, 100, 500, 500, name+ " Properties");

            propertiesWindow.AddComponent(new PhButton(300, 450, 80, 32, "OK", SaveProperties));
            propertiesWindow.AddComponent(new PhButton(400, 450, 80, 32, "Cancel", CloseProperties));

            int x = 100;
            int y = 30;
            if (component is Entity)
            {
                propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, (component as Entity).Position.X.ToString(), "X", PhTextEdit.ValueType.Float, null, null));
                y += 24;
                propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, (component as Entity).Position.Y.ToString(), "Y", PhTextEdit.ValueType.Float, null, null));
                y += 24;
                propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, MathHelper.ToDegrees((component as Entity).Orientation).ToString(), "Orientation", PhTextEdit.ValueType.Float, null, null));
                y += 24;
            }

            foreach (KeyValuePair<string, int> property in component.Properties.Ints)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, property.Value.ToString(), property.Key, PhTextEdit.ValueType.Int, null, null));
                    y+=24;
                    if (y>500-30) {
                        y = 30;
                        x += 200;
                    }
                }
            }

            foreach (KeyValuePair<string, float> property in component.Properties.Floats)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, property.Value.ToString(), property.Key, PhTextEdit.ValueType.Float, null, null));
                    y += 24;
                    if (y > 500 - 30)
                    {
                        y = 30;
                        x += 200;
                    }
                }
            }
            foreach (KeyValuePair<string, object> property in component.Properties.Objects)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    if (property.Value is string)
                    {
                        propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, property.Value as String, property.Key, PhTextEdit.ValueType.String, null, null));
                        y += 24;
                    }
                    if (property.Value is Color)
                    {
                        string hex = "#" + ((Color)property.Value).A.ToString("X2") + ((Color)property.Value).R.ToString("X2") + ((Color)property.Value).G.ToString("X2") + ((Color)property.Value).B.ToString("X2");
                        propertiesWindow.AddComponent(new PhTextEdit(x, y, 100, 20, hex, property.Key, PhTextEdit.ValueType.Color, null, null));
                        y += 24;
                    }
                    if (y > 500 - 30)
                    {
                        y = 30;
                        x += 200;
                    }
                }
            }

            windows.AddComponent(propertiesWindow);
        }

        private void SaveProperties(PhControl sender)
        {
            Component selectedComponent = propertiesWindowTarget;
            Entity selectedEntity = selectedComponent as Entity;
            if (selectedComponent == null)
                return;
            foreach (Component c in propertiesWindow.Components)
            {
                PhTextEdit edit = c as PhTextEdit;
                if (edit != null)
                {
                    if (edit.Caption == "X" && selectedEntity!=null)
                    {
                        float.TryParse(edit.Text, out selectedEntity.Position.X);
                    }
                    else if (edit.Caption == "Y" && selectedEntity != null)
                    {
                        float.TryParse(edit.Text, out selectedEntity.Position.Y);
                    }
                    else if (edit.Caption == "Orientation" && selectedEntity != null)
                    {
                        if (float.TryParse(edit.Text, out selectedEntity.Orientation))
                        {
                            selectedEntity.Orientation = MathHelper.ToRadians(selectedEntity.Orientation);
                        }
                    }
                    else
                    {

                        switch (edit.Type)
                        {
                            case PhTextEdit.ValueType.String:
                                selectedComponent.Properties.Objects[edit.Caption] = edit.Text;
                                break;
                            case PhTextEdit.ValueType.Int:
                                int i = 0;
                                int.TryParse(edit.Text, out i);
                                selectedComponent.Properties.Ints[edit.Caption] = i;
                                break;
                            case PhTextEdit.ValueType.Color:
                                int col = 0;
                                int.TryParse(edit.Text.Substring(1), NumberStyles.HexNumber, null, out col);
                                selectedComponent.Properties.Objects[edit.Caption] = col.ToColor();
                                break;
                            case PhTextEdit.ValueType.Float:
                                float f = 0;
                                float.TryParse(edit.Text, out f);
                                selectedComponent.Properties.Floats[edit.Caption] = f;
                                break;

                        }
                    }
                }
            }

            if (selectedEntity!=null) 
                selectedEntity.Integrate(0);
            selectedComponent.HandleMessage(Messages.PropertiesChanged, null);
            propertiesWindow.Hide();
            windows.RemoveComponent(propertiesWindow);
            propertiesWindow = null;
        }

        private void CloseProperties(PhControl sender)
        {
            propertiesWindow.Hide();
            windows.RemoveComponent(propertiesWindow);
            propertiesWindow = null;
        }

        private void MouseLeftDownTiles()
        {
            EntityLayer entities = layers[currentLayer].Layer as EntityLayer;
            if (entities != null)
            {
                int x = (int)Math.Floor(mousePosition.X / layers[currentLayer].TileSize);
                int y = (int)Math.Floor(mousePosition.Y / layers[currentLayer].TileSize);
                int index = x + y * tilesX;
                if (drawingType != null)
                {
                    if (tileMap[index] != null && tileMap[index].GetType() != drawingType.GetType()) 
                    {
                        tileMap[index].Destroyed = true;
                        entities.RemoveComponent(tileMap[index]);
                        tileMap[index] = null;
                    }

                    if (tileMap[index] == null)
                    {
                        Entity entity = EntityFactory.AssembleEntity(drawingType, drawingEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, ""));
                        entity.Position = SnapPosition(mousePosition);
                        entity.Properties.Ints["isTile"] = 1;
                        entities.AddComponent(entity);
                        tileMap[index] = entity;
                    }
                }
                else
                {
                    if (tileMap[index] != null)
                    {
                        tileMap[index].Destroyed = true;
                        entities.RemoveComponent(tileMap[index]);
                        tileMap[index] = null;
                    }
                }
            }
        }

        

        private void MouseRightDownTiles()
        {
            EntityLayer entities = layers[currentLayer].Layer as EntityLayer;
            if (entities != null)
            {
                int x = (int)Math.Floor(mousePosition.X / layers[currentLayer].TileSize);
                int y = (int)Math.Floor(mousePosition.Y / layers[currentLayer].TileSize);
                int index = x + y * tilesX;
                Entity entity = tileMap[index];
                if (entity != null)
                {
                    string blueprintName = entity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, "");
                    drawingType = layers[currentLayer].EntityList[blueprintName];
                    drawingEntity = EntityFactory.AssembleEntity(drawingType, blueprintName);
                    drawingEntity.Position = mousePosition;
                    return;
                }
                drawingEntity = null;
                drawingType = null;
            }
        }

        public override void Render(RenderInfo info)
        {
            if (info == null)
                return;
            info.Camera = Editing.Camera;
            if (layers[currentLayer].Layer is EntityLayer)
                DrawEntities(info, layers[currentLayer].Layer as EntityLayer);

            info.Batch.DrawString(font, layers[currentLayer].Name, new Vector2(11, PhantomGame.Game.Resolution.Height - 29), Color.Black);
            info.Batch.DrawString(font, layers[currentLayer].Name, new Vector2(10, PhantomGame.Game.Resolution.Height - 30), Color.White);
            base.Render(info);
            
        }

        private void DrawEntities(RenderInfo info, EntityLayer entities)
        {
            int tiles = layers[currentLayer].Type == LayerType.Entities ? 0: 1;
            Vector2 topLeft = new Vector2(info.Camera.Left, info.Camera.Top);
            for (int i = 0; i < entities.Components.Count; i++)
            {
                Entity entity = entities.Components[i] as Entity;
                if (entity != null && entity.Shape != null && entity.Properties.GetInt("isTile",0)==tiles)
                {
                    info.Canvas.StrokeColor = Color.White;
                    info.Canvas.LineWidth = 4;
                    topLeft = DrawEntity(info, topLeft, entity);

                    info.Canvas.StrokeColor = Color.Black;
                    if (entity == selectedEntity)
                        info.Canvas.StrokeColor = Color.Yellow;
                    else if (entity == hoveringEntity)
                        info.Canvas.StrokeColor = Color.Cyan;
                    info.Canvas.LineWidth = 2;
                    topLeft = DrawEntity(info, topLeft, entity);

                    if (windows.Ghost)
                    {
                        if (entity == selectedEntity)
                        {
                            string name = selectedEntity.GetType().Name;
                            Vector2 size = font.MeasureString(name);
                            info.Batch.DrawString(font, name, entity.Position - topLeft - size * 0.5f, Color.Yellow);
                        }
                        else if (entity == hoveringEntity)
                        {
                            string name = hoveringEntity.GetType().Name;
                            Vector2 size = font.MeasureString(name);
                            info.Batch.DrawString(font, name, entity.Position - topLeft - size * 0.5f, Color.Cyan);
                        }
                    }
                }
            }

            if (windows.Ghost)
            {
                info.Canvas.StrokeColor = Color.Red;
                info.Canvas.LineWidth = 2;
                if (drawingEntity != null && hoveringEntity == null)
                {
                    DrawEntity(info, topLeft, drawingEntity);
                    string name = drawingEntity.GetType().Name;
                    Vector2 size = font.MeasureString(name);
                    info.Batch.DrawString(font, name, drawingEntity.Position - topLeft - size * 0.5f, Color.Red);
                }
                else if (layers[currentLayer].Type == LayerType.Tiles)
                {
                    Vector2 pos = SnapPosition(mousePosition) - topLeft;
                    info.Canvas.Begin();
                    info.Canvas.MoveTo(pos - Vector2.One * layers[currentLayer].TileSize * 0.5f);
                    info.Canvas.LineTo(pos + Vector2.One * layers[currentLayer].TileSize * 0.5f);
                    info.Canvas.MoveTo(pos + new Vector2(layers[currentLayer].TileSize * 0.5f, -layers[currentLayer].TileSize * 0.5f));
                    info.Canvas.LineTo(pos + new Vector2(-layers[currentLayer].TileSize * 0.5f, layers[currentLayer].TileSize * 0.5f));
                    info.Canvas.Stroke();
                }
            }
        }

        private static Vector2 DrawEntity(RenderInfo info, Vector2 topLeft, Entity entity)
        {
            Circle circle = entity.Shape as Circle;
            OABB oabb = entity.Shape as OABB;
            Polygon polygon = entity.Shape as Polygon;
            if (circle != null)
            {
                info.Canvas.StrokeCircle(entity.Position - topLeft, circle.Radius);
                info.Canvas.StrokeLine(entity.Position - topLeft, entity.Position + entity.Direction * circle.Radius - topLeft);
            }
            else if (oabb != null)
            {
                info.Canvas.StrokeRect(entity.Position - topLeft, oabb.HalfSize, entity.Orientation);
            }
            else if (polygon != null)
            {
                Vector2 pos = entity.Position - topLeft;
                Vector2[] verts = polygon.RotatedVertices(entity.Orientation);
                info.Canvas.Begin();
                info.Canvas.MoveTo(pos + verts[verts.Length - 1]);
                for (int i = 0; i < verts.Length; i++)
                    info.Canvas.LineTo(pos + verts[i]);
                info.Canvas.Stroke();
            }
            return topLeft;
        }



    }
}
