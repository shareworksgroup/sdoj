#include "pch.h"
#include "sandbox.h"

using namespace std;


class scope_exit 
{
public:
	scope_exit(function<void()> f) :
		m_f(f)
	{
	}

	~scope_exit()
	{
		m_f();
	}

private:
	function<void()> m_f;
};



bool judge(api_judge_info const & aji, api_judge_result & ajr)
{
	try
	{
		judge_process ps{ aji };
		ps.execute();
		ps.get_result(ajr);
		return true;
	}
	catch (win32_exception const & e)
	{
		ajr.error_code = e.error_code;
		return false;
	}
	catch (...)
	{
		return false;
	}
}

void free_judge_result(api_judge_result & result)
{
	if (result.output != nullptr)
	{
		delete[] result.output;
		result.output = nullptr;
	}

	if (result.exception != nullptr)
	{
		delete[] result.exception;
		result.exception = nullptr;
	}
}

void __stdcall begin_run(api_run_info const & info, api_run_io_result & io_result)
{
	try
	{
		auto rp = new run_process(info);

		rp->begin_runs();

		// set io_result success at run_process
		rp->get_io_result(io_result);
		io_result.succeed = true;
	}
	catch (win32_exception const & e)
	{
		io_result.succeed = false;
		io_result.error_code = e.error_code;
	}
	catch (...)
	{
		io_result.succeed = false;
		io_result.error_code = GetLastError();
	}
}

void __stdcall end_run(run_process * run_process, api_run_result result)
{
	try
	{
		scope_exit se{ [=](){
			delete run_process; 
		} };

		run_process->end_runs();

		// set result success at run_process
		run_process->get_result(result);
		result.succeed = true;
	}
	catch (win32_exception const & e)
	{
		result.succeed = false;
		result.error_code = e.error_code;
	}
	catch (...)
	{
		result.succeed = false;
		result.error_code = GetLastError();
	}
}