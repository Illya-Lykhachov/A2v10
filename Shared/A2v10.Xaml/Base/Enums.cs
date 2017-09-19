﻿
namespace A2v10.Xaml
{
    public enum TextAlign
    {
        Default = 0,
        Left = Default,
        Right,
        Center
    }

    public enum VerticalAlign
    {
        Default,
        Top,
        Middle,
        Bottom
    }

    public enum GridLinesVisibility {
        None,
        Horizontal,
        Vertical,
        Both
    }

    public enum Orientation
    {
        Vertical,
        Horizontal
    }

    public enum DataType
    {
        String, 
        Date,
        DateTime,
        Time,
        Number,
        Currency
    }

    public enum ControlSize
    {
        Default = 0,
        Large = 1,
        Medium = Default,
        Small = 2,
        Mini = 3
    };

    public enum WrapMode
    {
        Default,
        Wrap,
        NoWrap,
        PreWrap
    }

    public enum Icon
    {
        NoIcon = 0,
        Alert,
        Approve,
        ArrowDown,
        ArrowLeft,
        ArrowRight,
        ArrowUp,
        Calendar,
        ChartArea,
        ChartBar,
        ChartColumn,
        ChartPie,
        ChartPivot,
        ChartStackedArea,
        ChartStackedBar,
        ChevronDown,
        ChevronLeft,
        ChevronLeftEnd,
        ChevronRight,
        ChevronRightEnd,
        ChevronUp,
        Clear,
        Close,
        Comment,
        CommentAdd,
        CommentDiscussion,
        CommentLines,
        CommentNext,
        CommentOutline,
        CommentPrevious,
        CommentUrgent,
        Copy,
        Cut,
        Delete,
        DeleteBox,
        Dashboard,
        Database,
        Disapprove,
        Dot,
        Download,
        Edit,
        Error,
        ErrorOutline,
        Exit,
        Eye,
        Flag,
        File,
        Filter,
        FilterOutline,
        Folder,
        Gear,
        GearOutline,
        Help,
        HelpOutline,
        History,
        Info,
        InfoOutline,
        Log,
        Menu,
        Message,
        Minus,
        MinusBox,
        Paste,
        Plus,
        PlusBox,
        Print,
        Refresh,
        Reload,
        Requery,
        Save,
        SaveClose,
        Search,
        Step,
        Steps,
        Success,
        SuccessOutline,
        Switch,
        Table,
        Tag,
        TagOutline,
        Trash,
        Upload,
        User,
        Users,
        Waiting,
        Warning,
        WarningOutline,
        Wrench
    }
}
