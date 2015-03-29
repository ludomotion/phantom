using System;
using System.Collections.Generic;
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
using System.Globalization;
using System.IO;
using Phantom.Cameras.Components;
using Phantom.GameUI;

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

        private static Phont font;

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

        private Window layerWindow;
        private Window propertiesWindow;
        private Window entitiesWindow;
        private Window main;
        private Component propertiesWindowTarget;


        public static void Initialize(Phont font)
        {
            MapLoader.Initialize();
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
            {
                this.Editing.Camera.HandleMessage(Messages.CameraStopFollowing, null);
                while (true)
                {
                    CameraOffset offset = this.Editing.Camera.GetComponentByType<CameraOffset>();
                    if (offset != null)
                        this.Editing.Camera.RemoveComponent(offset);
                    else
                        break;
                }
            }

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

            /*for (int i = editing.Components.Count - 1; i >= 0; i--)
            {
                Component c = editing.Components[i];
                if (c != editing.Camera && (c.Properties == null || c.Properties.GetString("editable", null) == null))
                    editing.RemoveComponent(c);
            }*/

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

            main = new Window(100, 100, 500, 500, "Main Menu");
            int y = 130;
            main.AddComponent(new Button(110, y += 32, 480, 24, "Layers", StartSelectLayer));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Entities", StartSelectEntity));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Clear Map", ClearMap));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Save Map", SaveMap));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Open Map", OpenMap));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Close Menu", CloseMenu));
            main.AddComponent(new Button(110, y += 32, 480, 24, "Close Editor", CloseEditor));

            layerWindow = new Window(100, 100, 500, 500, "Layers");
            layerWindow.Ghost = true;
            for (int i = 0; i < layers.Count; i++)
                layerWindow.AddComponent(new Button(110, 130+i*32, 480, 24, layers[i].Name, SelectLayer));
        }

        private int numberOfEntities = 13 * 3;
        private int firstEntity = 0;
        private Window BuildEntitiesWindow(Dictionary<string, PCNComponent> entityList, string name, int first)
        {
            int buttonWidth = 160;
            int buttonHeight = 24;
            int spacing = 8;
            int width = buttonWidth * 3 + spacing * 4;
            int height = 500;
            entitiesWindow = new Window(100, 100, width, height, name);
            entitiesWindow.Ghost = true;
            int x = spacing;
            int y = 20 + spacing;
            bool next = false;
            bool buttons = false;
            if (first == -1)
            {
                entitiesWindow.AddComponent(new Button(100+x, 100+y, buttonWidth, buttonHeight, "<none>", SelectEntity));
                y += buttonHeight + spacing;
                first++;
                buttons = true;
            }

            int i =first;
            foreach (KeyValuePair<string, PCNComponent> entity in entityList)
            {
                if (i > 0)
                {
                    i--;
                }
                else
                {
                    buttons = true;
                    entitiesWindow.AddComponent(new Button(100+x, 100+y, buttonWidth, buttonHeight, entity.Key, SelectEntity));
                    y += buttonHeight + spacing;
                    if (y > height - 2 * (buttonHeight + spacing))
                    {
                        x += buttonWidth + spacing;
                        y = 20 + spacing;
                    }

                    if (x > width - spacing-buttonWidth)
                    {
                        next = true;
                        break;
                    }
                }
            }

            if (first != 0)
                entitiesWindow.AddComponent(new Button(spacing, height - spacing - buttonHeight, buttonWidth, buttonHeight, "Previous", SelectPreviousEntities));
            if (next)
                entitiesWindow.AddComponent(new Button(width - spacing - buttonWidth, height - spacing - buttonHeight, buttonWidth, buttonHeight, "Next", SelectNextEntities));

            entitiesWindow.OnClose = CloseEntityWindow;

            if (!buttons && first > 0)
            {
                firstEntity -= numberOfEntities;
                return BuildEntitiesWindow(entityList, name, first - numberOfEntities);
            }
            else
                return entitiesWindow;
        }

        private void CloseEntityWindow(UIElement sender)
        {
            if (entitiesWindow != null)
            {
                entitiesWindow = null;
            }
        }

        private void SelectNextEntities(UIElement sender)
        {
            CloseEntityWindow(sender);
            firstEntity += numberOfEntities;
            StartSelectEntity(sender);
        }

        private void SelectPreviousEntities(UIElement sender)
        {
            CloseEntityWindow(sender);
            firstEntity -= numberOfEntities;
            StartSelectEntity(sender);
        }

        private void StartSelectEntity(UIElement sender)
        {
            if (layers[currentLayer].Type == LayerType.Tiles)
            {
                Window window = BuildEntitiesWindow(layers[currentLayer].EntityList, "Tiles", firstEntity-1);
                window.Show();
            }
            if (layers[currentLayer].Type == LayerType.Entities)
            {
                Window window = BuildEntitiesWindow(layers[currentLayer].EntityList, "Entities", firstEntity);
                window.Show();
            }
        }

        private void StartSelectLayer(UIElement sender)
        {
            layerWindow.Show();
        }

        private void ClearMap(UIElement sender)
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

        private void SaveMap(UIElement sender)
        {
            string filename = "";
            if (Editing.Properties != null)
                filename = Editing.Properties.GetString("filename", "");
#if DEBUG
            string searchname = Path.GetFileName(filename);
            string basedir = Path.GetDirectoryName(new Uri(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase).LocalPath);
            string startdir = Path.GetFullPath(Path.Combine(basedir, "../../../../"));

            if ((startdir.Split(Path.DirectorySeparatorChar).Length - 1) > 2) // If the startdir end up to be C:\...
            {
                string[] found = Directory.GetFiles(Path.Combine(basedir, "../../../../"), "*.contentproj", SearchOption.AllDirectories);
                if (found.Length > 0)
                {
                    found = Directory.GetFiles(Path.GetDirectoryName(found[0]), searchname, SearchOption.AllDirectories);
                    if (found.Length > 0)
                    {
                        Uri file = new Uri(Path.GetFullPath(found[0]));
                        Uri folder = new Uri(basedir.TrimEnd('/') + '/');
                        filename = Uri.UnescapeDataString(folder.MakeRelativeUri(file).ToString().Replace('/', Path.DirectorySeparatorChar));
                    }
                }
            }
#endif
            new InputDialog(100, 100, "Save Map", "Filename:", filename, ConfirmSave).Show();
        }

        private void ConfirmSave(UIElement sender)
        {
            MapLoader.SaveMap((sender as EditBox).Text);
        }


        private void OpenMap(UIElement sender)
        {
            string filename = "";
            if (Editing.Properties != null)
                filename = Editing.Properties.GetString("filename", "");
            new InputDialog(100, 100, "Open Map", "Filename:", filename, ConfirmOpen).Show();
        }

        private void ConfirmOpen(UIElement sender)
        {
            MapLoader.OpenMap((sender as EditBox).Text);
            if (Editing.Camera != null)
                this.Editing.Camera.HandleMessage(Messages.CameraStopFollowing, null);
        }

        private void CloseMenu(UIElement sender)
        {
            main.Hide();
        }


        private void SelectEntity(UIElement sender)
        {
            Button button = sender as Button;
            if (layers[currentLayer].EntityList != null)
            {
                drawingType = null;
                drawingEntity = null;
                if (layers[currentLayer].EntityList.ContainsKey(button.Name))
                {
                    drawingType = layers[currentLayer].EntityList[button.Name];
                    drawingEntity = EntityFactory.AssembleEntity(drawingType, button.Name);
                    drawingEntity.Position = mousePosition;
                }
            }
            button.GetAncestor<Window>().Hide();
            main.Hide();
        }

        private void SelectLayer(UIElement sender)
        {
            layerWindow.Hide();
            main.Hide();
            Button button = sender as Button;
            if (button != null)
            {
                for (int i = 0; i < layers.Count; i++)
                {
                    if (button.Name == layers[i].Name)
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


            if (Editing.Camera != null)
            {
                mousePosition.X = currentMouse.X + Editing.Camera.Left;
                mousePosition.Y = currentMouse.Y + Editing.Camera.Top;
            }
            else
            {
                mousePosition.X = currentMouse.X;
                mousePosition.Y = currentMouse.Y;
            }

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


            previousMouse = currentMouse;

            KeyboardState currentKeyboard = Keyboard.GetState();


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


            previousKeyboard = currentKeyboard;


        }



        private void CloseEditor(UIElement sender)
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
            propertiesWindow = new Window(100, 100, 500, 500, name+ " Properties");

            propertiesWindow.AddComponent(new Button(100 + 300, 100 + 450, 80, 32, "OK", SaveProperties));
            propertiesWindow.AddComponent(new Button(100 + 400, 100 + 450, 80, 32, "Cancel", CloseProperties));



            int x = 100;
            int y = 30;
            if (component is Entity)
            {
                propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, (component as Entity).Position.X.ToString(), "X", EditBox.ValueType.Float, null, null, null));
                y += 24;
                propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, (component as Entity).Position.Y.ToString(), "Y", EditBox.ValueType.Float, null, null, null));
                y += 24;
                propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, MathHelper.ToDegrees((component as Entity).Orientation).ToString(), "Orientation", EditBox.ValueType.Float, null, null, null));
                y += 24;
            }

            foreach (KeyValuePair<string, int> property in component.Properties.Ints)
            {
                if (property.Key[0] >= 'A' && property.Key[0] <= 'Z')
                {
                    propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, property.Value.ToString(), property.Key, EditBox.ValueType.Int, null, null, null));
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
                    propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, property.Value.ToString(), property.Key, EditBox.ValueType.Float, null, null, null));
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
                        propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, property.Value as String, property.Key, EditBox.ValueType.String, null, null, null));
                        y += 24;
                    }
                    if (property.Value is Color)
                    {
                        string hex = "#" + ((Color)property.Value).A.ToString("X2") + ((Color)property.Value).R.ToString("X2") + ((Color)property.Value).G.ToString("X2") + ((Color)property.Value).B.ToString("X2");
                        propertiesWindow.AddComponent(new EditBox(100 + x, 100 + y, 100, 20, hex, property.Key, EditBox.ValueType.Color, null, null, null));
                        y += 24;
                    }
                    if (y > 500 - 30)
                    {
                        y = 30;
                        x += 200;
                    }
                }
            }

            propertiesWindow.Show();
        }

        private void SaveProperties(UIElement sender)
        {
            Component selectedComponent = propertiesWindowTarget;
            Entity selectedEntity = selectedComponent as Entity;
            if (selectedComponent == null)
                return;
            foreach (Component c in propertiesWindow.Components)
            {
                EditBox edit = c as EditBox;
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
                            case EditBox.ValueType.String:
                                selectedComponent.Properties.Objects[edit.Caption] = edit.Text;
                                break;
                            case EditBox.ValueType.Int:
                                int i = 0;
                                int.TryParse(edit.Text, out i);
                                selectedComponent.Properties.Ints[edit.Caption] = i;
                                break;
                            case EditBox.ValueType.Color:
                                int col = 0;
                                int.TryParse(edit.Text.Substring(1), NumberStyles.HexNumber, null, out col);
                                selectedComponent.Properties.Objects[edit.Caption] = col.ToColor();
                                break;
                            case EditBox.ValueType.Float:
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
            propertiesWindow = null;
        }

        private void CloseProperties(UIElement sender)
        {
            propertiesWindow.Hide();
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
                if (index >= 0 && index < tileMap.Length)
                {
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

            font.DrawString(info, layers[currentLayer].Name, new Vector2(10, PhantomGame.Game.Resolution.Height - 30), Color.White);
            base.Render(info);
            
        }

        private void DrawEntities(RenderInfo info, EntityLayer entities)
        {
            int tiles = layers[currentLayer].Type == LayerType.Entities ? 0: 1;

            Vector2 topLeft = new Vector2(0, 0);
            if (Editing.Camera!=null) 
                topLeft = new Vector2(info.Camera.Left, info.Camera.Top);
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

                    if (info.IsTopState)
                    {
                        if (entity == selectedEntity)
                        {
                            string name = selectedEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, entity.GetType().Name);
                            Vector2 size = font.MeasureString(name);
                            font.DrawString(info, name, entity.Position - topLeft - size * 0.5f, Color.Yellow);
                        }
                        else if (entity == hoveringEntity)
                        {
                            string name = hoveringEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, hoveringEntity.GetType().Name);
                            Vector2 size = font.MeasureString(name);
                            font.DrawString(info, name, entity.Position - topLeft - size * 0.5f, Color.Cyan);
                        }
                    }
                }
            }

            if (info.IsTopState)
            {
                info.Canvas.StrokeColor = Color.Red;
                info.Canvas.LineWidth = 2;
                if (drawingEntity != null && hoveringEntity == null)
                {
                    DrawEntity(info, topLeft, drawingEntity);
                    string name = drawingEntity.Properties.GetString(EntityFactory.PROPERTY_NAME_BLUEPRINT, drawingEntity.GetType().Name); ;
                    Vector2 size = font.MeasureString(name);
                    font.DrawString(info, name, drawingEntity.Position - topLeft - size * 0.5f, Color.Red);
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
