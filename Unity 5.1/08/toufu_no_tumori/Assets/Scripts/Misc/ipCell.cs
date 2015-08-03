using UnityEngine;
using System.Collections;


public class ipCell {

#if true
	protected	float	min;
	protected	float	max;
	protected	float	current;

	private  ipCell() {}

	private void	init()
	{
		this.min = 0.0f;
		this.max = 1.0f;
		this.current = 0.0f;
	}


	public ipCell	setInput(float current)
	{
		this.current = current;

		return(this);
	}

	public float	getCurrent()
	{
		return(this.current);
	}

	// 0 ~ max 값으로 합니다(범위를 넘으면 반복합니다).
	public ipCell	repeat(float max)
	{
		this.current = Mathf.Repeat(this.current, max);

		this.min = 0.0f;
		this.max = max;

		return(this);
	}

	// 0 ~ 1.0 값으로 합니다.
	public ipCell	normalize()
	{
		this.current = Mathf.InverseLerp(this.min, this.max, this.current);

		this.min = 0.0f;
		this.max = 1.0f;

		return(this);
	}

	// 보간.
	public ipCell	lerp(float min, float max)
	{
		this.current = Mathf.Lerp(min, max, this.current);

		this.min = min;
		this.max = max;

		return(this);
	}

	// 0 ~ pi 값으로 합니다.
	public ipCell	uradian()
	{
		this.lerp(0.0f, Mathf.PI);

		return(this);
	}

	// 사인값.
	public ipCell	sin()
	{
		this.current = Mathf.Sin(this.current);

		this.max =  1.0f;
		this.min = -1.0f;

		return(this);
	}

	// quant의 정수배 값으로 절사.
	public ipCell	quantize(float quant)
	{
		this.current = Mathf.Floor(this.current/quant)*quant;

		return(this);
	}

	// ================================================================ //

	private	static ipCell	instance = null;

	public static ipCell	get()
	{
		if(ipCell.instance == null) {

			ipCell.instance = new ipCell();
		}

		return(ipCell.instance);
	}

#else
	public:

		 Atom() {}
		~Atom() {}

	public:
		ipModule&	clamp(float min, float max)
		{
			this->min = min;
			this->max = max;

			this->current = MathPrimary::minmax(this->min, this->current, this->max);

			return(*this);
		}
		ipModule&	delay(float delay)
		{
			this->current = std::max<float>(this->min, this->current - delay);

			return(*this);
		}

		ipModule&	remap(float new_min, float new_max)
		{


			this->current = MathPrimary::rate(this->min, this->max, this->current);

			this->max = new_max;
			this->min = new_min;

			this->current = MathPrimary::lerp(this->min, this->max, this->current);

			return(*this);
		}
		ipModule&	sin90(void)
		{
			this->remap(0.0f, 90.0f);

			this->current = MathPrimary::sinfDegree(this->current);

			this->max =  1.0f;
			this->min =  0.0f;

			return(*this);
		}
		ipModule&	square(void)
		{
			this->current = MathPrimary::square(this->current);

			return(*this);
		}
		ipModule&	sqrt(void)
		{
			this->current = sqrtf(this->current);

			return(*this);
		}
#endif
}

