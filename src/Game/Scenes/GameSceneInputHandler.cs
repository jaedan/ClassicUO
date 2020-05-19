#region license
// Copyright (C) 2020 ClassicUO Development Community on Github
// 
// This project is an alternative client for the game Ultima Online.
// The goal of this is to develop a lightweight client considering
// new technologies.
// 
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
// 
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
// 
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <https://www.gnu.org/licenses/>.
#endregion

using System;
using System.Linq;

using ClassicUO.Configuration;
using ClassicUO.Game.Data;
using ClassicUO.Game.GameObjects;
using ClassicUO.Game.Managers;
using ClassicUO.Game.UI.Controls;
using ClassicUO.Game.UI.Gumps;
using ClassicUO.Input;
using ClassicUO.IO.Resources;
using ClassicUO.Network;
using ClassicUO.Utility.Logging;

using Microsoft.Xna.Framework;

using SDL2;

using MathHelper = ClassicUO.Utility.MathHelper;

namespace ClassicUO.Game.Scenes
{
    internal partial class GameScene
    {
        private bool _followingMode;
        private uint _followingTarget;
        private bool _isSelectionActive;
        private readonly bool[] _requestedMovementDirections = new bool[4]; // Up, Down, Left, Right
        private bool _requestedWarMode;
        private bool _rightMousePressed, _continueRunning;
        private (int, int) _selectionStart, _selectionEnd;
        private uint _holdMouse2secOverItemTime;
        private bool _isMouseLeftDown;


        public bool IsMouseOverUI => UIManager.IsMouseOverAControl && !(UIManager.MouseOverControl is WorldViewport);
        public bool IsMouseOverViewport => UIManager.MouseOverControl is WorldViewport;

        private Direction _lastBoatDirection;
        private bool _boatRun, _boatIsMoving;

        private bool MoveCharacterByMouseInput()
        {
            if ((_rightMousePressed || _continueRunning) && World.InGame)// && !Pathfinder.AutoWalking)
            {
                if (Pathfinder.AutoWalking)
                    Pathfinder.StopAutoWalk();

                int x = ProfileManager.Current.GameWindowPosition.X + (ProfileManager.Current.GameWindowSize.X >> 1);
                int y = ProfileManager.Current.GameWindowPosition.Y + (ProfileManager.Current.GameWindowSize.Y >> 1);

                Direction direction = (Direction) GameCursor.GetMouseDirection(x, y, Mouse.Position.X, Mouse.Position.Y, 1);
                double mouseRange = MathHelper.Hypotenuse(x - Mouse.Position.X, y - Mouse.Position.Y);

                Direction facing = direction;

                if (facing == Direction.North)
                    facing = (Direction) 8;

                bool run = mouseRange >= 190;

                if (World.Player.IsDrivingBoat)
                {
                    if (!_boatIsMoving || _boatRun != run || _lastBoatDirection != facing - 1)
                    {
                        _boatRun = run;
                        _lastBoatDirection = facing - 1;
                        _boatIsMoving = true;

                        BoatMovingManager.MoveRequest(facing - 1, (byte) (run ? 2 : 1));
                    }
                }
                else
                    World.Player.Walk(facing - 1, run);

                return true;
            }

            return false;
        }

        private bool CanDragSelectOnObject(GameObject obj)
        {
            return obj is null || obj is Static || obj is Land || obj is Multi || obj is Item tmpitem && tmpitem.IsLocked;
        }

        private void SetDragSelectionStartEnd(ref (int, int) start, ref (int, int) end)
        {
            if (start.Item1 > Mouse.Position.X)
            {
                end.Item1 = start.Item1;
                start.Item1 = Mouse.Position.X;
            }
            else
                end.Item1 = Mouse.Position.X;

            if (start.Item2 > Mouse.Position.Y)
            {
                _selectionEnd.Item2 = start.Item2;
                start.Item2 = Mouse.Position.Y;
            }
            else
                end.Item2 = Mouse.Position.Y;
        }

        private bool DragSelectModifierActive()
        {
            // src: https://github.com/andreakarasho/ClassicUO/issues/621
            // drag-select should be disabled when using nameplates
            if (Keyboard.Ctrl && Keyboard.Shift)
                return false;

            if (ProfileManager.Current.DragSelectModifierKey == 0)
                return true;

            if (ProfileManager.Current.DragSelectModifierKey == 1 && Keyboard.Ctrl)
                return true;

            if (ProfileManager.Current.DragSelectModifierKey == 2 && Keyboard.Shift)
                return true;

            return false;
        }


        private void DoDragSelect()
        {
            SetDragSelectionStartEnd(ref _selectionStart, ref _selectionEnd);

            _rectangleObj.X = _selectionStart.Item1;
            _rectangleObj.Y = _selectionStart.Item2;
            _rectangleObj.Width = _selectionEnd.Item1 - _selectionStart.Item1;
            _rectangleObj.Height = _selectionEnd.Item2 - _selectionStart.Item2;

            int finalX = 100;
            int finalY = 100;

            bool useCHB = ProfileManager.Current.CustomBarsToggled;

            Rectangle rect = useCHB ? new Rectangle(0, 0, HealthBarGumpCustom.HPB_BAR_WIDTH, HealthBarGumpCustom.HPB_HEIGHT_MULTILINE) : GumpsLoader.Instance.GetTexture(0x0804).Bounds;

            foreach (Mobile mobile in World.Mobiles)
            {
                if (ProfileManager.Current.DragSelectHumanoidsOnly && !mobile.IsHuman)
                    continue;

                int x = ProfileManager.Current.GameWindowPosition.X + mobile.RealScreenPosition.X + (int) mobile.Offset.X + 22 + 5;
                int y = ProfileManager.Current.GameWindowPosition.Y + (mobile.RealScreenPosition.Y - (int) mobile.Offset.Z) + 22 + 5;

                x -= mobile.FrameInfo.X;
                y -= mobile.FrameInfo.Y;
                int w = mobile.FrameInfo.Width;
                int h = mobile.FrameInfo.Height;

                x = (int) (x * (1 / Scale));
                y = (int) (y * (1 / Scale));

                _rectanglePlayer.X = x;
                _rectanglePlayer.Y = y;
                _rectanglePlayer.Width = w;
                _rectanglePlayer.Height = h;

                if (_rectangleObj.Intersects(_rectanglePlayer))
                {
                    if (mobile != World.Player)
                    {
                        if (UIManager.GetGump<BaseHealthBarGump>(mobile) != null)
                        {
                            continue;
                        }

                        //Instead of destroying existing HP bar, continue if already opened.
                        GameActions.RequestMobileStatus(mobile);

                        BaseHealthBarGump hbgc;

                        if (useCHB)
                        {
                            hbgc = new HealthBarGumpCustom(mobile);
                        }
                        else
                        {
                            hbgc = new HealthBarGump(mobile);
                        }

                        if (finalY >= ProfileManager.Current.GameWindowPosition.Y + ProfileManager.Current.GameWindowSize.Y - 100)
                        {
                            finalY = 100;
                            finalX += rect.Width + 2;
                        }

                        if (finalX >= ProfileManager.Current.GameWindowPosition.X + ProfileManager.Current.GameWindowSize.X - 100)
                        {
                            finalX = 100;
                        }

                        hbgc.X = finalX;
                        hbgc.Y = finalY;


                        foreach (var bar in UIManager.Gumps
                                                .OfType<BaseHealthBarGump>()
                                                  //.OrderBy(s => mobile.NotorietyFlag)
                                                  //.OrderBy(s => s.ScreenCoordinateX) ///testing placement SYRUPZ SYRUPZ SYRUPZ
                                                  .OrderBy(s => s.ScreenCoordinateX)
                                                  .ThenBy(s => s.ScreenCoordinateY))
                        {
                            if (bar.Bounds.Intersects(hbgc.Bounds))
                            {
                                finalY = bar.Bounds.Bottom + 2;

                                if (finalY >= ProfileManager.Current.GameWindowPosition.Y + ProfileManager.Current.GameWindowSize.Y - 100)
                                {
                                    finalY = 100;
                                    finalX = bar.Bounds.Right + 2;
                                }

                                if (finalX >= ProfileManager.Current.GameWindowPosition.X + ProfileManager.Current.GameWindowSize.X - 100)
                                {
                                    finalX = 100;
                                }

                                hbgc.X = finalX;
                                hbgc.Y = finalY;
                            }
                        }


                        finalY += rect.Height + 2;


                        UIManager.Add(hbgc);

                        hbgc.SetInScreen();
                    }
                }
            }

            _isSelectionActive = false;
        }

        internal override bool OnLeftMouseDown()
        {
            if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
                UIManager.ShowGamePopup(null);

            if (!IsMouseOverViewport)
                return false;

            if (World.CustomHouseManager != null)
            {
                _isMouseLeftDown = true;

                if (TargetManager.IsTargeting &&
                    TargetManager.TargetingState == CursorTarget.MultiPlacement &&
                    (World.CustomHouseManager.SelectedGraphic != 0 ||
                     World.CustomHouseManager.Erasing ||
                     World.CustomHouseManager.SeekTile) &&
                    SelectedObject.LastObject is GameObject obj)
                {
                    World.CustomHouseManager.OnTargetWorld(obj);
                    _lastSelectedMultiPositionInHouseCustomization.X = obj.X;
                    _lastSelectedMultiPositionInHouseCustomization.Y = obj.Y;
                }
            }
            else
            {
                _dragginObject = SelectedObject.Object as Entity;

                if (ProfileManager.Current.EnableDragSelect && DragSelectModifierActive())
                {
                    if (CanDragSelectOnObject(SelectedObject.Object as GameObject))
                    {
                        _selectionStart = (Mouse.Position.X, Mouse.Position.Y);
                        _isSelectionActive = true;
                    }
                }
                else
                {
                    _isMouseLeftDown = true;
                    _holdMouse2secOverItemTime = Time.Ticks;
                }
            }

            return true;
        }

        internal override bool OnLeftMouseUp()
        {
            if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
                UIManager.ShowGamePopup(null);

            if (_isMouseLeftDown)
            {
                _isMouseLeftDown = false;
                _holdMouse2secOverItemTime = 0;
            }

            //  drag-select code comes first to allow selection finish on mouseup outside of viewport
            if (_selectionStart.Item1 == Mouse.Position.X && _selectionStart.Item2 == Mouse.Position.Y)
                _isSelectionActive = false;

            if (_isSelectionActive)
            {
                DoDragSelect();

                return true;
            }

            if (!IsMouseOverViewport)
            {
                if (ItemHold.Enabled)
                {
                    UIManager.MouseOverControl?.InvokeMouseUp(Mouse.Position, MouseButtonType.Left);

                    return true;
                }

                return false;
            }

            if (_rightMousePressed)
                _continueRunning = true;

            if (_dragginObject != null)
                _dragginObject = null;

            if (UIManager.IsDragging)
                return false;

            if (ItemHold.Enabled)
            {
                if (SelectedObject.Object is GameObject obj && obj.Distance < Constants.DRAG_ITEMS_DISTANCE)
                {
                    switch (obj)
                    {
                        case Mobile mobile:
                            MergeHeldItem(mobile);

                            break;

                        case Item item:

                            if (item.IsCorpse)
                                MergeHeldItem(item);
                            else
                            {
                                SelectedObject.Object = item;

                                if (item.Graphic == ItemHold.Graphic && ItemHold.IsStackable)
                                    MergeHeldItem(item);
                                else
                                    DropHeldItemToWorld(obj.X, obj.Y, (sbyte) (obj.Z + item.ItemData.Height));
                            }

                            break;

                        case Multi multi:
                            DropHeldItemToWorld(obj.X, obj.Y, (sbyte) (obj.Z + multi.ItemData.Height));

                            break;

                        case Static st:
                            DropHeldItemToWorld(obj.X, obj.Y, (sbyte) (obj.Z + st.ItemData.Height));

                            break;

                        case Land _:
                            DropHeldItemToWorld(obj.X, obj.Y, obj.Z);

                            break;

                        default:
                            Log.Warn("Unhandled mouse inputs for GameObject type " + obj.GetType());

                            return false;
                    }
                }
                else
                    Client.Game.Scene.Audio.PlaySound(0x0051);
            }
            else if (TargetManager.IsTargeting)
            {
                switch (TargetManager.TargetingState)
                {
                    case CursorTarget.Grab:
                    case CursorTarget.SetGrabBag:
                    case CursorTarget.Position:
                    case CursorTarget.Object:
                    case CursorTarget.MultiPlacement when World.CustomHouseManager == null:
                    {
                        var obj = SelectedObject.Object;
                        if (obj is TextOverhead ov)
                            obj = ov.Owner;
                        else if (obj is GameEffect eff && eff.Source != null)
                            obj = eff.Source;

                        switch (obj)
                        {
                            case Entity ent:
                                TargetManager.Target(ent.Serial);
                                break;
                            case Land land:
                                TargetManager.Target(0, land.X, land.Y, land.Z, land.TileData.IsWet);
                                break;
                            case GameObject o:
                                TargetManager.Target(o.Graphic, o.X, o.Y, o.Z);
                                break;
                        }
                    }

                    Mouse.LastLeftButtonClickTime = 0;
                    break;

                    case CursorTarget.SetTargetClientSide:
                    {
                        var obj = SelectedObject.Object;
                        if (obj is TextOverhead ov)
                            obj = ov.Owner;
                        else if (obj is GameEffect eff && eff.Source != null)
                            obj = eff.Source;

                        switch (obj)
                        {
                            case Entity ent:
                                TargetManager.Target(ent.Serial);
                                UIManager.Add(new InspectorGump(ent));
                                break;
                            case Land land:
                                TargetManager.Target(0, land.X, land.Y, land.Z);
                                UIManager.Add(new InspectorGump(land));
                                break;
                            case GameObject o:
                                TargetManager.Target(o.Graphic, o.X, o.Y, o.Z);
                                UIManager.Add(new InspectorGump(o));
                                break;
                        }

                        Mouse.LastLeftButtonClickTime = 0;
                    }

                    break;

                    case CursorTarget.HueCommandTarget:

                        if (SelectedObject.Object is Entity selectedEntity)
                        {
                            CommandManager.OnHueTarget(selectedEntity);
                        }

                        break;
                }
            }
            else
            {
                GameObject obj = SelectedObject.LastObject as GameObject;

                switch (obj)
                {
                    case Static st:
                        string name = st.Name;
                        if (string.IsNullOrEmpty(name))
                            name = ClilocLoader.Instance.GetString(1020000 + st.Graphic, st.ItemData.Name);
                        obj.AddMessage(MessageType.Label, name, 3, 1001, false);


                        if (obj.TextContainer != null && obj.TextContainer.MaxSize != 1)
                            obj.TextContainer.MaxSize = 1;
                        break;

                    case Multi multi:
                        name = multi.Name;

                        if (string.IsNullOrEmpty(name))
                            name = ClilocLoader.Instance.GetString(1020000 + multi.Graphic, multi.ItemData.Name);
                        obj.AddMessage(MessageType.Label, name, 3, 1001, false);

                        if (obj.TextContainer != null && obj.TextContainer.MaxSize == 5)
                            obj.TextContainer.MaxSize = 1;
                        break;

                    case Entity ent:

                        if (Keyboard.Alt && ent is Mobile)
                        {
                            World.Player.AddMessage(MessageType.Regular, "Now following.", 3, 1001, false);
                            _followingMode = true;
                            _followingTarget = ent;
                        }
                        else if (!DelayedObjectClickManager.IsEnabled)
                        {
                            DelayedObjectClickManager.Set(ent.Serial, Mouse.Position.X, Mouse.Position.Y, Time.Ticks + Mouse.MOUSE_DELAY_DOUBLE_CLICK);
                        }

                        break;
                }
            }

            return true;
        }

        internal override bool OnLeftMouseDoubleClick()
        {
            bool result = false;

            if (!IsMouseOverViewport)
            {
                result = DelayedObjectClickManager.IsEnabled;

                if (result)
                {
                    DelayedObjectClickManager.Clear();

                    return false;
                }

                return false;
            }
            else
            {
                BaseGameObject obj = SelectedObject.LastObject;

                switch (obj)
                {
                    case Item item:
                        result = true;
                        if (!GameActions.OpenCorpse(item))
                            GameActions.DoubleClick(item);
                        break;

                    case Mobile mob:
                        result = true;

                        if (World.Player.InWarMode && World.Player != mob)
                            GameActions.Attack(mob);
                        else
                            GameActions.DoubleClick(mob);
                        break;

                    case TextOverhead msg when msg.Owner is Entity entity:
                        result = true;
                        GameActions.DoubleClick(entity);
                        break;
                    default:
                        World.LastObject = 0;
                        break;
                }
            }

            if (result)
            {
                DelayedObjectClickManager.Clear();
            }

            return result;
        }


        internal override bool OnRightMouseDown()
        {
            if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
                UIManager.ShowGamePopup(null);

            if (!IsMouseOverViewport)
                return false;

            _rightMousePressed = true;
            _continueRunning = false;
            StopFollowing();

            return true;
        }


        internal override bool OnRightMouseUp()
        {
            if (UIManager.PopupMenu != null && !UIManager.PopupMenu.Bounds.Contains(Mouse.Position.X, Mouse.Position.Y))
                UIManager.ShowGamePopup(null);

            _rightMousePressed = false;

            if (_boatIsMoving)
            {
                _boatIsMoving = false;
                BoatMovingManager.MoveRequest(World.Player.Direction, 0);
            }

            return !IsMouseOverUI;
        }


        internal override bool OnRightMouseDoubleClick()
        {
            if (!IsMouseOverViewport)
                return false;

            if (ProfileManager.Current.EnablePathfind && !Pathfinder.AutoWalking)
            {
                if (ProfileManager.Current.UseShiftToPathfind && !Keyboard.Shift)
                    return false;

                if (SelectedObject.Object is GameObject obj)
                {
                    if (obj is Static || obj is Multi || obj is Item)
                    {
                        ref readonly var itemdata = ref TileDataLoader.Instance.StaticData[obj.Graphic];

                        if (itemdata.IsSurface && Pathfinder.WalkTo(obj.X, obj.Y, obj.Z, 0))
                        {
                            World.Player.AddMessage(MessageType.Label, "Pathfinding!", 3, 1001, false);
                            return true;
                        }
                    }
                    else if (obj is Land && Pathfinder.WalkTo(obj.X, obj.Y, obj.Z, 0))
                    {
                        World.Player.AddMessage(MessageType.Label, "Pathfinding!", 3, 1001, false);
                        return true;
                    }
                }

                //if (SelectedObject.Object is Land || GameObjectHelper.TryGetStaticData(SelectedObject.Object as GameObject, out var itemdata) && itemdata.IsSurface)
                //{
                //    if (SelectedObject.Object is GameObject obj && Pathfinder.WalkTo(obj.X, obj.Y, obj.Z, 0))
                //    {

                //    }
                //}
            }

            return false;
        }



        internal override bool OnMouseWheel(bool up)
        {
            if (!IsMouseOverViewport)
                return false;

            if (ProfileManager.Current.EnableMousewheelScaleZoom)
            {
                if (!Keyboard.Ctrl)
                    return false;

                if (!up)
                    ZoomOut();
                else
                    ZoomIn();

                return true;
            }

            return false;
        }


        internal override bool OnMouseDragging()
        {
            if (!IsMouseOverViewport)
                return false;

            if (Mouse.LButtonPressed && !ItemHold.Enabled)
            {
                Point offset = Mouse.LDroppedOffset;

                if (Math.Abs(offset.X) > Constants.MIN_PICKUP_DRAG_DISTANCE_PIXELS || Math.Abs(offset.Y) > Constants.MIN_PICKUP_DRAG_DISTANCE_PIXELS)
                {
                    Entity obj;
                    if (ProfileManager.Current.SallosEasyGrab && SelectedObject.LastObject is Entity ent && _dragginObject == null)
                    {
                        obj = ent;
                    }
                    else
                    {
                        obj = _dragginObject;
                    }

                    if (obj != null)
                    {
                        if (SerialHelper.IsMobile(obj.Serial) || obj is Item it && it.IsDamageable)
                        {
                            GameActions.RequestMobileStatus(obj);
                            var customgump = UIManager.GetGump<BaseHealthBarGump>(obj);
                            customgump?.Dispose();

                            if (obj == World.Player)
                                StatusGumpBase.GetStatusGump()?.Dispose();

                            if (ProfileManager.Current.CustomBarsToggled)
                            {
                                Rectangle rect = new Rectangle(0, 0, HealthBarGumpCustom.HPB_WIDTH, HealthBarGumpCustom.HPB_HEIGHT_SINGLELINE);
                                UIManager.Add(customgump = new HealthBarGumpCustom(obj) { X = Mouse.LDropPosition.X - (rect.Width >> 1), Y = Mouse.LDropPosition.Y - (rect.Height >> 1) });
                            }
                            else
                            {
                                Rectangle rect = GumpsLoader.Instance.GetTexture(0x0804).Bounds;
                                UIManager.Add(customgump = new HealthBarGump(obj) { X = Mouse.LDropPosition.X - (rect.Width >> 1), Y = Mouse.LDropPosition.Y - (rect.Height >> 1) });
                            }
                            UIManager.AttemptDragControl(customgump, Mouse.Position, true);
                        }
                        else if (obj is Item item)
                        {
                            PickupItemBegin(item, Mouse.Position.X, Mouse.Position.Y);
                        }
                    }

                    _dragginObject = null;
                }
            }

            return true;
        }

        internal void OnMovement(Direction dir, bool keyDown)
        {
            switch (dir & Direction.Mask)
            {
                case Direction.Up:
                    _requestedMovementDirections[0] = keyDown;
                    break;
                case Direction.Down:
                    _requestedMovementDirections[1] = keyDown;
                    break;
                case Direction.Left:
                    _requestedMovementDirections[2] = keyDown;
                    break;
                case Direction.Right:
                    _requestedMovementDirections[3] = keyDown;
                    break;
            }
        }
      
        internal override void OnKeyDown(SDL.SDL_KeyboardEvent e)
        {
            if (UIManager.KeyboardFocusControl != UIManager.SystemChat.TextBoxControl)
                return;

            // Some keys require more complex handling separate from the keybind manager.
            // Deal with them here.
            switch (e.keysym.sym)
            {
                case SDL.SDL_Keycode.SDLK_ESCAPE:
                    if (TargetManager.IsTargeting)
                    {
                        TargetManager.CancelTarget();
                        break;
                    }

                    if (Pathfinder.AutoWalking && Pathfinder.PathindingCanBeCancelled)
                    {
                        Pathfinder.StopAutoWalk();
                        break;
                    }

                    break;

                // chat system activation

                case SDL.SDL_Keycode.SDLK_1 when Keyboard.IsModPressed(e.keysym.mod, SDL.SDL_Keymod.KMOD_SHIFT): // !
                case SDL.SDL_Keycode.SDLK_BACKSLASH when Keyboard.IsModPressed(e.keysym.mod, SDL.SDL_Keymod.KMOD_SHIFT): // \

                    if (ProfileManager.Current.ActivateChatAfterEnter && ProfileManager.Current.ActivateChatAdditionalButtons && !UIManager.SystemChat.IsActive)
                        UIManager.SystemChat.IsActive = true;

                    break;

                case SDL.SDL_Keycode.SDLK_EXCLAIM: // !
                case SDL.SDL_Keycode.SDLK_SEMICOLON: // ;
                case SDL.SDL_Keycode.SDLK_COLON: // :
                case SDL.SDL_Keycode.SDLK_SLASH: // /
                case SDL.SDL_Keycode.SDLK_BACKSLASH: // \
                case SDL.SDL_Keycode.SDLK_PERIOD: // .
                case SDL.SDL_Keycode.SDLK_KP_PERIOD: // .
                case SDL.SDL_Keycode.SDLK_COMMA: // ,
                case SDL.SDL_Keycode.SDLK_LEFTBRACKET: // [
                case SDL.SDL_Keycode.SDLK_MINUS: // -
                case SDL.SDL_Keycode.SDLK_KP_MINUS: // -
                    if (ProfileManager.Current.ActivateChatAfterEnter &&
                        ProfileManager.Current.ActivateChatAdditionalButtons && !UIManager.SystemChat.IsActive)
                    {
                        if (Keyboard.IsModPressed(e.keysym.mod, SDL.SDL_Keymod.KMOD_NONE))
                            UIManager.SystemChat.IsActive = true;
                        else if (Keyboard.IsModPressed(e.keysym.mod, SDL.SDL_Keymod.KMOD_SHIFT) && e.keysym.sym == SDL.SDL_Keycode.SDLK_SEMICOLON)
                            UIManager.SystemChat.IsActive = true;
                    }
                    break;
                case SDL.SDL_Keycode.SDLK_RETURN:
                case SDL.SDL_Keycode.SDLK_KP_ENTER:
                    if (ProfileManager.Current.ActivateChatAfterEnter)
                    {
                        UIManager.SystemChat.Mode = ChatMode.Default;

                        if (!(Keyboard.IsModPressed(e.keysym.mod, SDL.SDL_Keymod.KMOD_SHIFT) && ProfileManager.Current.ActivateChatShiftEnterSupport))
                            UIManager.SystemChat.ToggleChatVisibility();
                    }

                    return;
            }

            if (UIManager.SystemChat.IsActive && ProfileManager.Current.ActivateChatAfterEnter)
                return;

            if (KeyBinds.OnKeyDown(e.keysym.sym, e.keysym.mod))
                return;
        }


        internal override void OnKeyUp(SDL.SDL_KeyboardEvent e)
        {
            if (ProfileManager.Current.EnableMousewheelScaleZoom && ProfileManager.Current.RestoreScaleAfterUnpressCtrl && !Keyboard.Ctrl)
                Scale = ProfileManager.Current.DefaultScale;

            if (KeyBinds.OnKeyUp(e.keysym.sym, e.keysym.mod))
                return;
        }
    }
}