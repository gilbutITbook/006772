using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// 스플라인 처리 클래스.
public class SplineData
{
	public const int	SEND_INTERVAL = 10;
	
	#if true
	public void CalcSpline(List<CharacterCoord> dPoint)
	{
		m_points.Clear();
		
		Vector3		ps = Vector3.zero;
		Vector3		pe = Vector3.zero;
		
		Vector3		vs = Vector3.zero;
		Vector3		ve = Vector3.zero;
		
		if(dPoint.Count == 0) {
			
		} else if(dPoint.Count == 1) {
			
			ps = dPoint[dPoint.Count - 1].ToVector3();
			pe = ps;
			
			vs = Vector3.zero;
			ve = Vector3.zero;
			
		} else if(dPoint.Count == 2) {
			
			ps = dPoint[dPoint.Count - 2].ToVector3();
			pe = dPoint[dPoint.Count - 1].ToVector3();
			
			vs = ps - pe;
			ve = vs;
			
		} else if(dPoint.Count == 3) {
			
			ps = dPoint[dPoint.Count - 2].ToVector3();
			pe = dPoint[dPoint.Count - 1].ToVector3();
			
			vs = (dPoint[dPoint.Count - 2].ToVector3() - dPoint[dPoint.Count - 3].ToVector3());
			
			Vector3		v0 = (dPoint[dPoint.Count - 2].ToVector3() - dPoint[dPoint.Count - 3].ToVector3());
			Vector3		v1 = (dPoint[dPoint.Count - 1].ToVector3() - dPoint[dPoint.Count - 2].ToVector3());
			
			Vector3		v2 = v1 + (v1 - v0);
			
			ve = v2;
			
		} else if(dPoint.Count >= 4) {
			
			ps = dPoint[dPoint.Count - 2].ToVector3();
			pe = dPoint[dPoint.Count - 1].ToVector3();
			
			vs = (dPoint[dPoint.Count - 2].ToVector3() - dPoint[dPoint.Count - 3].ToVector3());
			
			Vector3		v0 = (dPoint[dPoint.Count - 3].ToVector3() - dPoint[dPoint.Count - 4].ToVector3());
			Vector3		v1 = (dPoint[dPoint.Count - 2].ToVector3() - dPoint[dPoint.Count - 3].ToVector3());
			Vector3		v2 = (dPoint[dPoint.Count - 1].ToVector3() - dPoint[dPoint.Count - 2].ToVector3());
			
			Vector3		dv0 = v1 - v0;
			Vector3		dv1 = v2 - v1;
			
			Vector3		dv2 = dv1 + (dv1 - dv0);
			
			Vector3		v3 = v2 + dv2;
			
			ve = v3;
		}
		
		SimpleSpline.Curve	spline = new SimpleSpline.Curve();
		
		spline.appendCV(ps, vs*0.5f);
		spline.appendCV(pe, ve*0.5f);
		
		SimpleSpline.Tracer		tracer = new SimpleSpline.Tracer();
		
		tracer.attach(spline);
		
		float	total_dist = spline.calcTotalDistance();
		
		for(int i = 0;i < SEND_INTERVAL-1;i++) {
			
			float		rate = ((float)i)/((float)SEND_INTERVAL);
			
			tracer.proceedToDistance(total_dist*rate);
			
			m_points.Add(CharacterCoord.FromVector3(tracer.getCurrent().position));
		}
	}
	
	public int		GetPlotNum()
	{
		return(m_points.Count);
	} 
	public void		GetPoint(int index, out CharacterCoord coord)
	{
		coord = m_points[index];
	} 
	
	protected List<CharacterCoord>		m_points = new List<CharacterCoord>();
	
	#else
	public void CalcSpline(List<CharacterCoord> dPoint, int cullingNum) {}
	public int		GetPlotNum() { return(0); } 
	public void		GetPoint(int index, ref CharacterCoord coord) {} 
	#endif
}
