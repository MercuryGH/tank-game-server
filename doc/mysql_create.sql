-- MySQL建数据库 + 表语句

create database tank_game;

-- 玩家账号

create table account(
    id varchar(20),
    pw varchar(40) not null,
    primary key (id)
);

-- 玩家个人信息

create table player(
    id varchar(20),
    info json,
    foreign key (id) references account(id)
);

-- 居然在MySQL里面用 JSON，MongoDB狂喜