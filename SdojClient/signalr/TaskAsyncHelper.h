//Copyright (c) Microsoft Corporation
//
//All rights reserved.
//
//THIS CODE IS PROVIDED ON AN *AS IS* BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY, OR NON-INFRINGEMENT.

#pragma once

#include <cpprest\basic_types.h>
#include <cpprest\asyncrt_utils.h>
#include "timer_t.h"

namespace MicrosoftAspNetSignalRClientCpp
{
    enum TaskStatus
    {
        TaskFaulted = 0,
        TaskCanceled,
        TaskCompleted
    };

    // TaskAsyncHelper::Delay is experimental
    template <typename T>
    class DelayedTaskHelper
    {
    public:
        ~DelayedTaskHelper()
        {
        }

        static pplx::task<T> Create(utility::seconds delay, pplx::cancellation_token ct)
        {
            // using raw pointers here, ported from casablanca cpp rest sdk
            auto helper = new DelayedTaskHelper<T>();

            if (ct != pplx::cancellation_token::none())
            {
                pplx::cancellation_token_registration registration = ct.register_callback([helper]()
                {
                    helper->mTimer.stop(false);
                    delete helper;
                });
                
                helper->DeregisterCallback = [ct, registration]()
                {
                    ct.deregister_callback(registration);
                };
            }

            auto task = pplx::create_task(helper->mTce);
            helper->mTimer.start(delay.count()*1000, false, TimerCallback, (void *)helper);
            return task;
        }

        static pplx::task<void> CreateVoid(utility::seconds delay, pplx::cancellation_token ct)
        {
            // using raw pointers here, ported from casablanca cpp rest sdk
            auto helper = new DelayedTaskHelper<void>();

            if (ct != pplx::cancellation_token::none())
            {
                pplx::cancellation_token_registration registration = ct.register_callback([helper]()
                {
                    helper->mTimer.stop(false);
                    delete helper;
                });

                helper->DeregisterCallback = [ct, registration]()
                {
                    ct.deregister_callback(registration);
                };
            }

            auto task = pplx::create_task(helper->mTce);
            helper->mTimer.start(static_cast<unsigned>(delay.count())*1000, false, TimerCallbackVoid, (void *)helper);
            return task;
        }

    private:
        DelayedTaskHelper()
        {
            DeregisterCallback = [](){};
        }
    
        static void TimerCallback(void* context)
        {
            auto helper = static_cast<DelayedTaskHelper<T>*>(context);
            helper->DeregisterCallback();
            helper->mTce.set(T());
            delete helper;
        }

        static void TimerCallbackVoid(void* context)
        {
            auto helper = static_cast<DelayedTaskHelper<void>*>(context);
            helper->DeregisterCallback();
            helper->mTce.set();
            delete helper;
        }

        pplx::task_completion_event<T> mTce;
        timer_t mTimer;
        std::function<void()> DeregisterCallback;
    };

    class TaskAsyncHelper
    {
    public:
        TaskAsyncHelper();
        ~TaskAsyncHelper();

        // TaskAsyncHelper::Delay is experimental
        static pplx::task<void> Delay(utility::seconds seconds, pplx::cancellation_token ct = pplx::cancellation_token::none());

        template <typename T1>
        static pplx::task<T1> Delay(utility::seconds seconds, pplx::cancellation_token ct = pplx::cancellation_token::none())
        {
            return DelayedTaskHelper<T1>::Create(seconds, ct).then([](T1 retval){
                return retval;
            }, ct);
        }

        template <typename T2>
        static TaskStatus RunTaskToCompletion(pplx::task<T2> task, T2& result, std::exception& ex)
        {
            try
            {
                result = task.get();
                return TaskStatus::TaskCompleted;
            }
            catch(pplx::task_canceled canceled)
            {
                return TaskStatus::TaskCanceled;
            }
            catch(exception& exception)
            {
                ex = exception;
                return TaskStatus::TaskFaulted;
            }
        }
    };
} // namespace MicrosoftAspNetSignalRClientCpp