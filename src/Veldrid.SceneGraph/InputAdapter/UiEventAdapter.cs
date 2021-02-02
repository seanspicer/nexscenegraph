

using System;
using System.Linq;

namespace Veldrid.SceneGraph.InputAdapter
{
    public interface IUiEventAdapter : IEvent 
    {
        [Flags]
        enum ModKeyMaskType
        {
            ModKeyLeftShift  = 0x0001,
            ModKeyRightShift = 0x0002,
            ModKeyLeftCtl    = 0x0004,
            ModKeyRightCtl   = 0x0008,
            ModKeyLeftAlt    = 0x0010,
            ModKeyRightAlt   = 0x0020,
            ModKeyLeftMeta   = 0x0040,
            ModKeyRightMeta  = 0x0080,
            ModKeyLeftSuper  = 0x0100,
            ModKeyRightSuper = 0x0200,
            ModKeyLeftHyper  = 0x0400,
            ModKeyRightHyper = 0x0800,
            ModKeyNumLock    = 0x1000,
            ModKeyCapsLock   = 0x2000,
            ModKeyCtl        = (ModKeyLeftCtl|ModKeyRightCtl),
            ModKeyShift      = (ModKeyLeftShift|ModKeyRightShift),
            ModKeyAlt        = (ModKeyLeftAlt|ModKeyRightAlt),
            ModKeyMeta       = (ModKeyLeftMeta|ModKeyRightMeta),
            ModKeySuper      = (ModKeyLeftSuper|ModKeyRightSuper),
            ModKeyHyper      = (ModKeyLeftHyper|ModKeyRightHyper)
        };
        
        ModKeyMaskType ModKeyMask { get; set; }
        
        [Flags]
        enum MouseButtonMaskType {
            None               = 0,
            LeftMouseButton    = 1<<0,
            MiddleMouseButton  = 1<<1,
            RightMouseButton   = 1<<2
        };
        
        MouseButtonMaskType MouseButtonMask { get; set; }

        [Flags]
        enum EventTypeValue {
            None                = 0,
            Push                = 1<<0,
            Release             = 1<<1,
            DoubleClick         = 1<<2,
            Drag                = 1<<3,
            Move                = 1<<4,
            KeyDown             = 1<<5,
            KeyUp               = 1<<6,
            Frame               = 1<<7,
            Resize              = 1<<8,
            Scroll              = 1<<9,
            PenPressure         = 1<<10,
            PenOrientation      = 1<<11,
            PenProximityEnter   = 1<<12,
            PenProximityLeave   = 1<<13,
            CloseWindow         = 1<<14,
            QuitApplication     = 1<<15,
            User                = 1<<16
        };
        
        EventTypeValue EventType { get; set; } 
        
        [Flags]
        enum KeySymbol
        {
            KeySpace           = 0x20,
            Key0               = '0',
            Key1               = '1',
            Key2               = '2',
            Key3               = '3',
            Key4               = '4',
            Key5               = '5',
            Key6               = '6',
            Key7               = '7',
            Key8               = '8',
            Key9               = '9',
            KeyA               = 'a',
            KeyB               = 'b',
            KeyC               = 'c',
            KeyD               = 'd',
            KeyE               = 'e',
            KeyF               = 'f',
            KeyG               = 'g',
            KeyH               = 'h',
            KeyI               = 'i',
            KeyJ               = 'j',
            KeyK               = 'k',
            KeyL               = 'l',
            KeyM               = 'm',
            KeyN               = 'n',
            KeyO               = 'o',
            KeyP               = 'p',
            KeyQ               = 'q',
            KeyR               = 'r',
            KeyS               = 's',
            KeyT               = 't',
            KeyU               = 'u',
            KeyV               = 'v',
            KeyW               = 'w',
            KeyX               = 'x',
            KeyY               = 'y',
            KeyZ               = 'z',

            KeyExclaim         = 0x21,
            KeyQuotedbl        = 0x22,
            KeyHash            = 0x23,
            KeyDollar          = 0x24,
            KeyAmpersand       = 0x26,
            KeyQuote           = 0x27,
            KeyLeftparen       = 0x28,
            KeyRightparen      = 0x29,
            KeyAsterisk        = 0x2A,
            KeyPlus            = 0x2B,
            KeyComma           = 0x2C,
            KeyMinus           = 0x2D,
            KeyPeriod          = 0x2E,
            KeySlash           = 0x2F,
            KeyColon           = 0x3A,
            KeySemicolon       = 0x3B,
            KeyLess            = 0x3C,
            KeyEquals          = 0x3D,
            KeyGreater         = 0x3E,
            KeyQuestion        = 0x3F,
            KeyAt              = 0x40,
            KeyLeftbracket     = 0x5B,
            KeyBackslash       = 0x5C,
            KeyRightbracket    = 0x5D,
            KeyCaret           = 0x5E,
            KeyUnderscore      = 0x5F,
            KeyBackquote       = 0x60,

            KeyBackSpace       = 0xFF08,        /* back space, back char */
            KeyTab             = 0xFF09,
            KeyLinefeed        = 0xFF0A,        /* Linefeed, LF */
            KeyClear           = 0xFF0B,
            KeyReturn          = 0xFF0D,        /* Return, enter */
            KeyPause           = 0xFF13,        /* Pause, hold */
            KeyScrollLock      = 0xFF14,
            KeySysReq          = 0xFF15,
            KeyEscape          = 0xFF1B,
            KeyDelete          = 0xFFFF,        /* Delete, rubout */


            /* Cursor control & motion */

            KeyHome            = 0xFF50,
            KeyLeft            = 0xFF51,        /* Move left, left arrow */
            KeyUp              = 0xFF52,        /* Move up, up arrow */
            KeyRight           = 0xFF53,        /* Move right, right arrow */
            KeyDown            = 0xFF54,        /* Move down, down arrow */
            KeyPrior           = 0xFF55,        /* Prior, previous */
            KeyPageUp         = 0xFF55,
            KeyNext            = 0xFF56,        /* Next */
            KeyPageDown       = 0xFF56,
            KeyEnd             = 0xFF57,        /* EOL */
            KeyBegin           = 0xFF58,        /* BOL */


            /* Misc Functions */

            KeySelect          = 0xFF60,        /* Select, mark */
            KeyPrint           = 0xFF61,
            KeyExecute         = 0xFF62,        /* Execute, run, do */
            KeyInsert          = 0xFF63,        /* Insert, insert here */
            KeyUndo            = 0xFF65,        /* Undo, oops */
            KeyRedo            = 0xFF66,        /* redo, again */
            KeyMenu            = 0xFF67,        /* On Windows, this is VK_APPS, the context-menu key */
            KeyFind            = 0xFF68,        /* Find, search */
            KeyCancel          = 0xFF69,        /* Cancel, stop, abort, exit */
            KeyHelp            = 0xFF6A,        /* Help */
            KeyBreak           = 0xFF6B,
            KeyModeSwitch     = 0xFF7E,        /* Character set switch */
            KeyScriptSwitch   = 0xFF7E,        /* Alias for mode_switch */
            KeyNumLock        = 0xFF7F,

            /* Keypad Functions, keypad numbers cleverly chosen to map to ascii */

            KeyKpSpace        = 0xFF80,        /* space */
            KeyKpTab          = 0xFF89,
            KeyKpEnter        = 0xFF8D,        /* enter */
            KeyKpF1           = 0xFF91,        /* PF1, KP_A, ... */
            KeyKpF2           = 0xFF92,
            KeyKpF3           = 0xFF93,
            KeyKpF4           = 0xFF94,
            KeyKpHome         = 0xFF95,
            KeyKpLeft         = 0xFF96,
            KeyKpUp           = 0xFF97,
            KeyKpRight        = 0xFF98,
            KeyKpDown         = 0xFF99,
            KeyKpPrior        = 0xFF9A,
            KeyKpPageUp      = 0xFF9A,
            KeyKpNext         = 0xFF9B,
            KeyKpPageDown    = 0xFF9B,
            KeyKpEnd          = 0xFF9C,
            KeyKpBegin        = 0xFF9D,
            KeyKpInsert       = 0xFF9E,
            KeyKpDelete       = 0xFF9F,
            KeyKpEqual        = 0xFFBD,        /* equals */
            KeyKpMultiply     = 0xFFAA,
            KeyKpAdd          = 0xFFAB,
            KeyKpSeparator    = 0xFFAC,       /* separator, often comma */
            KeyKpSubtract     = 0xFFAD,
            KeyKpDecimal      = 0xFFAE,
            KeyKpDivide       = 0xFFAF,

            KeyKp0            = 0xFFB0,
            KeyKp1            = 0xFFB1,
            KeyKp2            = 0xFFB2,
            KeyKp3            = 0xFFB3,
            KeyKp4            = 0xFFB4,
            KeyKp5            = 0xFFB5,
            KeyKp6            = 0xFFB6,
            KeyKp7            = 0xFFB7,
            KeyKp8            = 0xFFB8,
            KeyKp9            = 0xFFB9,

            /*
             * Auxiliary Functions; note the duplicate definitions for left and right
             * function keys;  Sun keyboards and a few other manufactures have such
             * function key groups on the left and/or right sides of the keyboard.
             * We've not found a keyboard with more than 35 function keys total.
             */

            KeyF1              = 0xFFBE,
            KeyF2              = 0xFFBF,
            KeyF3              = 0xFFC0,
            KeyF4              = 0xFFC1,
            KeyF5              = 0xFFC2,
            KeyF6              = 0xFFC3,
            KeyF7              = 0xFFC4,
            KeyF8              = 0xFFC5,
            KeyF9              = 0xFFC6,
            KeyF10             = 0xFFC7,
            KeyF11             = 0xFFC8,
            KeyF12             = 0xFFC9,
            KeyF13             = 0xFFCA,
            KeyF14             = 0xFFCB,
            KeyF15             = 0xFFCC,
            KeyF16             = 0xFFCD,
            KeyF17             = 0xFFCE,
            KeyF18             = 0xFFCF,
            KeyF19             = 0xFFD0,
            KeyF20             = 0xFFD1,
            KeyF21             = 0xFFD2,
            KeyF22             = 0xFFD3,
            KeyF23             = 0xFFD4,
            KeyF24             = 0xFFD5,
            KeyF25             = 0xFFD6,
            KeyF26             = 0xFFD7,
            KeyF27             = 0xFFD8,
            KeyF28             = 0xFFD9,
            KeyF29             = 0xFFDA,
            KeyF30             = 0xFFDB,
            KeyF31             = 0xFFDC,
            KeyF32             = 0xFFDD,
            KeyF33             = 0xFFDE,
            KeyF34             = 0xFFDF,
            KeyF35             = 0xFFE0,

            /* Modifiers */

            KeyShiftL         = 0xFFE1,        /* Left shift */
            KeyShiftR         = 0xFFE2,        /* Right shift */
            KeyControlL       = 0xFFE3,        /* Left control */
            KeyControlR       = 0xFFE4,        /* Right control */
            KeyCapsLock       = 0xFFE5,        /* Caps lock */
            KeyShiftLock      = 0xFFE6,        /* Shift lock */

            KeyMetaL          = 0xFFE7,        /* Left meta */
            KeyMetaR          = 0xFFE8,        /* Right meta */
            KeyAltL           = 0xFFE9,        /* Left alt */
            KeyAltR           = 0xFFEA,        /* Right alt */
            KeySuperL         = 0xFFEB,        /* Left super */
            KeySuperR         = 0xFFEC,        /* Right super */
            KeyHyperL         = 0xFFED,        /* Left hyper */
            KeyHyperR         = 0xFFEE         /* Right hyper */
        };
        
        KeySymbol Key { get; set; }
        
        enum ScrollingMotionType
        {
            ScrollNone,
            ScrollLeft,
            ScrollRight,
            ScrollUp,
            ScrollDown,
            Scroll2D
        };
        
        ScrollingMotionType ScrollingMotion { get; set; }
        
        PointerDataList PointerDataList { get; set; }
        
        PointerData GetPointerData(IObject obj);
        void AddPointerData(PointerData pd);
        
        float X { get; set; }
        float Y { get; set; }

        float XMax { get; set; }
        float XMin { get; set; }
        float YMax { get; set; }
        float YMin { get; set; }
        
        float XNormalized { get; }
        float YNormalized { get; }
    }
    
    public class UiEventAdapter : Event, IUiEventAdapter
    {
        public IUiEventAdapter.ModKeyMaskType ModKeyMask { get; set; }
        public IUiEventAdapter.EventTypeValue EventType { get; set; }
        
        public IUiEventAdapter.KeySymbol Key { get; set; }

        public IUiEventAdapter.MouseButtonMaskType MouseButtonMask { get; set; } =
            IUiEventAdapter.MouseButtonMaskType.None;

        public IUiEventAdapter.ScrollingMotionType ScrollingMotion { get; set; }
        
        public PointerDataList PointerDataList { get; set; } = new PointerDataList();

        public float X { get; set; } = 0.0f;

        public float Y { get; set; } = 0.0f;
        public float XMax { get; set; } = 1.0f;
        public float XMin { get; set; } = -1.0f;
        public float YMax { get; set; } = 1.0f;
        public float YMin { get; set; } = -1.0f;

        public float XNormalized =>
            PointerDataList.Count > 0
                ? PointerDataList.Last().GetXNormalized()
                : 2.0f * (X - XMin) / (XMax - XMin) - 1.0f;

        public float YNormalized =>
            PointerDataList.Count > 0
                ? PointerDataList.Last().GetYNormalized()
                : -2.0f * (Y - YMin) / (YMax - YMin) + 1.0f;

        public PointerData GetPointerData(IObject obj)
        {
            return PointerDataList.FirstOrDefault(x => x.Object == obj);
        }

        public void AddPointerData(PointerData pd)
        {
            PointerDataList.Add(pd);
        }

        public static IUiEventAdapter Create()
        {
            return new UiEventAdapter();
        }

        protected UiEventAdapter()
        {
            
        }
    }
}