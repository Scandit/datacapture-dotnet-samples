// WARNING
//
// This file has been generated automatically by Visual Studio from the outlets and
// actions declared in your storyboard file.
// Manual changes to this file will not be maintained.
//
using System.CodeDom.Compiler;

namespace SearchAndFindSample.ViewControllers;

[Register("SearchViewController")]
partial class SearchViewController
{
    [Outlet]
    public UIButton dismissOverlayButton { get; set; } = null!;

    [Outlet]
    public UIView scannedBarcodeOverlay { get; set; } = null!;

    [Outlet]
    public UILabel scannedBarcodeLabel { get; set; } = null!;

    void ReleaseDesignerOutlets()
    {
    }
}
