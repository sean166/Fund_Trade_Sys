CREATE TABLE tblClient (client_id INT PRIMARY KEY IDENTITY(1,1),
	first_name VARCHAR(50) NOT NULL,
	last_name VARCHAR(50) NOT NULL,
	age INT,
	gender VARCHAR(10));

CREATE TABLE instrument (
	fund_id INT PRIMARY KEY IDENTITY(1,1),
	fund_name VARCHAR(50) NOT NULL,
	fund_code VARCHAR(50) NOT NULL);

CREATE TABLE securities(
	price_id INT PRIMARY KEY IDENTITY(1,1),
	fund_id INT NOT NULL,
	CONSTRAINT fk_instrument FOREIGN KEY (fund_id)
	REFERENCES instrument(fund_id),
	price DECIMAL(18,2) NOT NULL,
	fund_code VARCHAR(50) NOT NULL
	);

CREATE TABLE SECClientAccount(
	account_id INT PRIMARY KEY IDENTITY(1,1),
	account_num BIGINT NOT NULL,
	opentime DATETIME NOT NULL,
	closetime DATETIME,
	acc_status VARCHAR(50) NOT NULL,
	balance DECIMAL(18,2),
	client_id INT NOT NULL,
	CONSTRAINT fk_tblClient
	FOREIGN KEY(client_id) REFERENCES tblClient(client_id));

CREATE TABLE cashEntry(
	trans_id INT PRIMARY KEY IDENTITY(1,1),
	money_in DECIMAL(18,2),
	money_out DECIMAL(18,2),
	account_id INT NOT NULL,
	CONSTRAINT fk_SECClientAccount
	FOREIGN KEY(account_id) REFERENCES SECClientAccount(account_id));

CREATE TABLE orders(
	order_id INT PRIMARY KEY IDENTITY(1,1),
	fund_name VARCHAR(50) NOT NULL,
	amount DECIMAL(18,2) NOT NULL,
	units INT NOT NULL,
	price DECIMAL(18,2) NOT NULL,
	order_time DATETIME NOT NULL,
	order_status VARCHAR(50) NOT NULL,
	account_id INT NOT NULL,
	CONSTRAINT fk_order_SECClientAccount
	FOREIGN KEY(account_id) REFERENCES SECClientAccount(account_id));


CREATE TABLE clientHoldFunds(
	chf_id INT PRIMARY KEY IDENTITY(1,1),
	ave_price DECIMAL(18,2) NOT NULL,
	amount DECIMAL(18,2) NOT NULL,
	units INT NOT NULL,
	fund_name VARCHAR(50) NOT NULL,
	account_id INT NOT NULL,
	CONSTRAINT fk_chf_SECClientAccount
	FOREIGN KEY(account_id) REFERENCES SECClientAccount(account_id));

Create Table margin(
	margin_id INT PRIMARY KEY IDENTITY(1,1),
	margin_date DATETIME NOT NULL,
	fund_code varchar(50) NOT NULL	,
	profit decimal(18,2),
	curAvePrice decimal(18,2),
	markPrice decimal(18,2),
	curAmount decimal(18,2),
	holdUnits int,
	tradeUnits int,
	account_id INT NOT NULL,
	CONSTRAINT fk_margin_SECClientAccount
	FOREIGN KEY(account_id) REFERENCES SECClientAccount(account_id));

