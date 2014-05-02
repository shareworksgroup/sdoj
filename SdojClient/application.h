#pragma once
#include "app_config.h"

struct application
{
	app_config config;
	shared_ptr<MicrosoftAspNetSignalRClientCpp::Connection> connection;

	void run();
};