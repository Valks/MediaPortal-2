#region Copyright (C) 2007-2010 Team MediaPortal

/*
    Copyright (C) 2007-2010 Team MediaPortal
    http://www.team-mediaportal.com

    This file is part of MediaPortal 2

    MediaPortal 2 is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    MediaPortal 2 is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with MediaPortal 2. If not, see <http://www.gnu.org/licenses/>.
*/

#endregion

using MediaPortal.Core;
using MediaPortal.Core.General;
using MediaPortal.UI.SkinEngine.ContentManagement;
using MediaPortal.UI.SkinEngine.Controls.Visuals;
using MediaPortal.UI.SkinEngine.DirectX;
using MediaPortal.UI.SkinEngine.Rendering;
using SlimDX;
using SlimDX.Direct3D9;
using MediaPortal.Utilities.DeepCopy;

namespace MediaPortal.UI.SkinEngine.Controls.Brushes
{
  // TODO: Implement Freezable behaviour
  public class LinearGradientBrush : GradientBrush
  {
    #region Consts

    protected const string EFFECT_LINEARGRADIENT = "lineargradient";
    protected const string EFFECT_LINEAROPACITYGRADIENT = "lineargradient_opacity";

    protected const string PARAM_TRANSFORM = "g_transform";
    protected const string PARAM_OPACITY = "g_opacity";
    protected const string PARAM_STARTPOINT = "g_startpoint";
    protected const string PARAM_ENDPOINT = "g_endpoint";

    protected const string PARAM_ALPHATEX = "g_alphatex";
    protected const string PARAM_UPPERVERTSBOUNDS = "g_uppervertsbounds";
    protected const string PARAM_LOWERVERTSBOUNDS = "g_lowervertsbounds";

    #endregion

    #region Private fields

    EffectAsset _effect;

    AbstractProperty _startPointProperty;
    AbstractProperty _endPointProperty;
    GradientBrushTexture _gradientBrushTexture;
    float[] g_startpoint;
    float[] g_endpoint;
    bool _refresh = false;

    #endregion

    #region Ctor

    public LinearGradientBrush()
    {
      Init();
      Attach();
    }

    public override void Dispose()
    {
      base.Dispose();
      Detach();
    }

    void Init()
    {
      _startPointProperty = new SProperty(typeof(Vector2), new Vector2(0.0f, 0.0f));
      _endPointProperty = new SProperty(typeof(Vector2), new Vector2(1.0f, 1.0f));
    }

    void Attach()
    {
      _startPointProperty.Attach(OnPropertyChanged);
      _endPointProperty.Attach(OnPropertyChanged);
    }

    void Detach()
    {
      _startPointProperty.Detach(OnPropertyChanged);
      _endPointProperty.Detach(OnPropertyChanged);
    }

    public override void DeepCopy(IDeepCopyable source, ICopyManager copyManager)
    {
      Detach();
      base.DeepCopy(source, copyManager);
      LinearGradientBrush b = (LinearGradientBrush) source;
      StartPoint = copyManager.GetCopy(b.StartPoint);
      EndPoint = copyManager.GetCopy(b.EndPoint);
      Attach();
    }

    #endregion

    protected override void OnPropertyChanged(AbstractProperty prop, object oldValue)
    {
      _refresh = true;
      FireChanged();
    }

    public AbstractProperty StartPointProperty
    {
      get { return _startPointProperty; }
    }

    public Vector2 StartPoint
    {
      get { return (Vector2) _startPointProperty.GetValue(); }
      set { _startPointProperty.SetValue(value); }
    }

    public AbstractProperty EndPointProperty
    {
      get { return _endPointProperty; }
    }

    public Vector2 EndPoint
    {
      get { return (Vector2) _endPointProperty.GetValue(); }
      set { _endPointProperty.SetValue(value); }
    }

    public override void SetupBrush(FrameworkElement parent, ref PositionColoredTextured[] verts, float zOrder, bool adaptVertsToBrushTexture)
    {
      base.SetupBrush(parent, ref verts, zOrder, adaptVertsToBrushTexture);
      _refresh = true;
    }

    public override bool BeginRenderBrush(PrimitiveBuffer primitiveContext, RenderContext renderContext)
    {
      if (_gradientBrushTexture == null || _refresh)
      {
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        if (_gradientBrushTexture == null)
          return false;
      }

      Matrix finalTransform = renderContext.Transform.Clone();
      if (_refresh)
      {
        _refresh = false;
        _effect = ServiceRegistration.Get<ContentManager>().GetEffect(EFFECT_LINEARGRADIENT);

        g_startpoint = new float[] {StartPoint.X, StartPoint.Y};
        g_endpoint = new float[] {EndPoint.X, EndPoint.Y};
        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_startpoint[0] /= _vertsBounds.Width;
          g_startpoint[1] /= _vertsBounds.Height;

          g_endpoint[0] /= _vertsBounds.Width;
          g_endpoint[1] /= _vertsBounds.Height;
        }
        if (RelativeTransform != null)
        {
          Matrix m = RelativeTransform.GetTransform();
          m.Transform(ref g_startpoint[0], ref g_startpoint[1]);
          m.Transform(ref g_endpoint[0], ref g_endpoint[1]);
        }
      }

      _effect.Parameters[PARAM_TRANSFORM] = GetCachedFinalBrushTransform();
      _effect.Parameters[PARAM_OPACITY] = (float) (Opacity * renderContext.Opacity);
      _effect.Parameters[PARAM_STARTPOINT] = g_startpoint;
      _effect.Parameters[PARAM_ENDPOINT] = g_endpoint;

      GraphicsDevice.Device.SetSamplerState(0, SamplerState.AddressU, SpreadAddressMode);
      _effect.StartRender(_gradientBrushTexture.Texture, finalTransform);
      return true;
    }

    public override void BeginRenderOpacityBrush(Texture tex, RenderContext renderContext)
    {
      if (tex == null)
        return;
      if (_gradientBrushTexture == null || _refresh)
      {
        _gradientBrushTexture = BrushCache.Instance.GetGradientBrush(GradientStops);
        if (_gradientBrushTexture == null)
          return;
      }

      Matrix finalTransform = renderContext.Transform.Clone();
      if (_refresh)
      {
        _refresh = false;
        _effect = ServiceRegistration.Get<ContentManager>().GetEffect(EFFECT_LINEAROPACITYGRADIENT);

        g_startpoint = new float[] {StartPoint.X, StartPoint.Y};
        g_endpoint = new float[] {EndPoint.X, EndPoint.Y};
        if (MappingMode == BrushMappingMode.Absolute)
        {
          g_startpoint[0] /= _vertsBounds.Width;
          g_startpoint[1] /= _vertsBounds.Height;

          g_endpoint[0] /= _vertsBounds.Width;
          g_endpoint[1] /= _vertsBounds.Height;
        }

        if (RelativeTransform != null)
        {
          Matrix m = RelativeTransform.GetTransform();
          m.Transform(ref g_startpoint[0], ref g_startpoint[1]);
          m.Transform(ref g_endpoint[0], ref g_endpoint[1]);
        }
      }
      SurfaceDescription desc = tex.GetLevelDescription(0);
      float[] g_LowerVertsBounds = new float[] {_vertsBounds.Left / desc.Width, _vertsBounds.Top / desc.Height};
      float[] g_UpperVertsBounds = new float[] {_vertsBounds.Right / desc.Width, _vertsBounds.Bottom / desc.Height};

      _effect.Parameters[PARAM_TRANSFORM] = GetCachedFinalBrushTransform();
      _effect.Parameters[PARAM_OPACITY] = (float) (Opacity * renderContext.Opacity);
      _effect.Parameters[PARAM_STARTPOINT] = g_startpoint;
      _effect.Parameters[PARAM_ENDPOINT] = g_endpoint;
      _effect.Parameters[PARAM_ALPHATEX] = _gradientBrushTexture.Texture;
      _effect.Parameters[PARAM_UPPERVERTSBOUNDS] = g_UpperVertsBounds;
      _effect.Parameters[PARAM_LOWERVERTSBOUNDS] = g_LowerVertsBounds;

      GraphicsDevice.Device.SetSamplerState(0, SamplerState.AddressU, SpreadAddressMode);
      _effect.StartRender(tex, finalTransform);
    }

    public override void EndRender()
    {
      if (_effect != null)
        _effect.EndRender();
    }

    public override Texture Texture
    {
      get { return _gradientBrushTexture.Texture; }
    }
  }
}
