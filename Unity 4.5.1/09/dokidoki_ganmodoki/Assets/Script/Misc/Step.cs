using UnityEngine;
using System.Collections;

// 실행 스텝 관리.
public class Step<T> where T : struct {

	public Step(T none)
	{
		this.none = none;
		init();
	}

	public void	init()
	{
		this.previous = this.none;
		this.current  = this.none;
		this.next     = this.none;

		this.previous_time = -1.0f;
		this.time = 0.0f;
		this.count = 0;

		this.status.is_changed = false;

		this.delay.delay = -1.0f;
		this.delay.next  = this.none;
	}

	public void	release()
	{
		this.init();
	}

	// 다음 스텝을 설정한다.
	public void	set_next(T step)
	{
		this.next = step;
	}
	// 다음 스텝을 가져온다.
	public T	get_next()
	{
		return(this.next);
	}

	// delay[sec] 기다렸다가 다음 스텝으로 전환한다.
	public void	set_next_delay(T step, float delay)
	{
		this.next = this.none;

		this.delay.delay = delay;
		this.delay.next  = step;
	}

	// 현재 스텝을 가져온다.
	public T	get_current()
	{
		return(this.current);
	}
	// 이전 스텝을 가져온다.
	public T	get_previous()
	{
		return(this.previous);
	}

	// 스텝이 전환된 순간?.
	public bool	is_changed()
	{
		return(this.status.is_changed);
	}

	// [sec] 스텝 내 경과 시간을 가져온다.
	public float	get_time()
	{
		return(this.time);
	}

	// [sec] 직전 실행 시 경과 시간을 가져온다.
	public float	get_previous_time()
	{
		return(this.previous_time);
	}

	// 전환판정.
	public T	do_transition()
	{
#if true
		return(this.do_transition_internal());
#else
		T	step;

		step = this.current;

		return(step);
#endif
	}

	// 전환 판정(내부 전환만).
	public T	do_transition_internal()
	{
		T	step;

		if(!this.delay.next.Equals(this.none)) {

			step = this.none;

			if(this.delay.delay <= 0.0f) {

				this.next = this.delay.next;
				this.delay.delay = -1.0f;
				this.delay.next  = this.none;
			}

		} else {

			if(this.next.Equals(this.none)) {
	
				step = this.current;
	
			} else {
	
				// 전환이 결정되어 있는(외부 요청).
				// 경우는 하지 않는다.
	
				step = this.none;
			}
		}

		return(step);
	}

	// 시작.
	public T		do_initialize()
	{
		T	step;

		if(!this.next.Equals(this.none)) {

			step = this.next;

			this.previous = this.current;
			this.current  = this.next;
			this.next     = this.none;
			this.time     = -1.0f;
			this.count    = 0;

			this.status.is_changed = true;

		} else {

			// 시작할 것이 없다(전환이 일어나지 않았다).
			//
			step = this.none;

			this.status.is_changed = false;
		}

		return(step);
	}

	// 실행.
	public T		do_execution(float passage_time)
	{
		T	step;

		if(this.delay.delay >= 0.0f) {

			this.delay.delay -= passage_time;

			step = this.none;

		} else {

			if(!this.current.Equals(this.none)) {
	
				step = this.current;
	
			} else {
	
				step = this.none;
			}
	
			this.count++;
	
			this.previous_time = this.time;
	
			if(this.time < 0.0f) {
	
				this.time = 0.0f;
	
			} else {
	
				this.time += passage_time;
			}
		}

		return(step);
	}

	// 일시정지(스텝 없음)하고 싶을 때 사용한다.
	public void		sleep()
	{
		this.current = this.none;
	}

	// 시각 time을 건너는 순간?.
	public bool		is_acrossing_time(float time)
	{
		bool	ret = (this.previous_time < time && time <= this.time);

		return(ret);
	}

	public bool		is_acrossing_cycle(float cycle)
	{
		bool	ret = (Mathf.Ceil(this.previous_time/cycle) < Mathf.Ceil(this.time/cycle));

		return(ret);
	}

	// ---------------------------------------------------------------- //

	protected	T		previous;
	protected	T		current;
	protected	T		next;

	protected	T		none;

	protected 	float		time;					// STEP가 변하고 난 후의 경과 시간.
	protected 	float		previous_time;			// 직전에 do_execution()했을 때의 time.
	protected 	int			count;

	protected struct Status {

		public	bool		is_changed;
	};
	protected Status	status;

	protected struct Delay {

		public float		delay;
		public T			next;
	};
	protected Delay	delay;
};

// 사용법.
#if false

		// ---------------------------------------------------------------- //
		// 다음 상태로 전환할지 체크.

		switch(this.step.do_transition()) {

			case STEP.IDLE:
			{
			}
			break;
		}

		// ---------------------------------------------------------------- //
		// 상태 전환 시 초기화.

		while(this.step.get_next() != STEP.NONE) {

			switch(this.step.do_initialize()) {
	
				case STEP.STAND:
				{
				}
				break;
			}
		}

		// ---------------------------------------------------------------- //
		// 각 상태에서의 실행 처리.

		switch(this.step.do_execution(Time.deltaTime)) {

			case STEP.STAND:
			{
			}
			break;
		}

#endif

