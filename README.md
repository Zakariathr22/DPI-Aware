This repository is a try to provide

* A workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/10452
* An implementation intended for integration into WinUI-Gallery to resolve https://github.com/microsoft/WinUI-Gallery/issues/1891

---

### FINAL CONCLUSION
The `AppWindow` and `OverlappedPresenter` APIs operate in **physical pixels**, not DIPs. Passing raw DIP values leads to inconsistent sizing across different DPI settings (e.g., 100% vs 150% scaling).

For example, setting a min width of 500 DIPs will look smaller on a high-DPI screen unless it’s scaled to match the system’s physical pixel density.


Using `PreferredMinimumWidth`, `PreferredMinimumHeight`, `PreferredMaximumWidth`, and `PreferredMaximumHeight` and trying to work around DPI differences issue is *possible*, but far from ideal because it will make us write a very complex code even if it will work as expected (like what we have in this repo).

The better approach is to skip these APIs entirely and use a custom method like the [SetWindowMinMaxSize](https://github.com/microsoft/WinUI-Gallery/blob/b68128fa24726436b8d7353386fea006e5d21e0d/WinUIGallery/Helpers/Win32WindowHelper.cs#L22) implementation used in the WinUI 3 Gallery.
