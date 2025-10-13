# Ergonomic Data sample

## ***说明:***

这些数据是我在XREAL作为人体工学工程师收集并处理的Ergonomic Data的样本。原始数据由扫描上百名志愿者的扫描电子模型组成，包括亚洲人与非亚洲人。数据收集的初衷为AR眼镜的外形设计提供Ergonomic参考，例如head breadth，Nasal root width，Alar slop angles，distance between tragion and cornea. etc。原始数据最终全部通过面部landmark 标记与数据提取（by Python）目标数据，最终生成目标数据报告与平局模型。这些数据与报告仍然被用于XREAL的设计参考。出于数据归属权和版权原因，此数据样本仅会展示部分数据展示的图片以及全部由我个人完成的数据提取脚本。展示过程中出现的任何个人模型或者人像，均是我本人的人像资料，不会涉及任何数据志愿者的隐私。注：若存在任何侵权或法律问题，请联系我以删除此展示。

 "*UNKNOWN*"

https://github.com/DerrickHHY/PhD-Application-Demo/issues/2

## ***Raw Data Sample:***

所有的原始数据都是通过3D扫描仪器对志愿者完成头部的扫面。扫描过程中会通过物理的方式完成一些矫正，以弥补深度摄影机制的本质缺陷，例如耳根与头部相连处的深度信息失真。所有的原始头模都经过我个人的矫正与处理，包括水平校准，坐标系对其，关键面部landmark 标定，移除无关信息等。样板头模的图如下，3D文件信息可见XXfile中的XX文件。

## ***数据处理流程:***

公司内并没有相关人士提供指导，所以我只能根据文献中的方法学来建立我自己的工作流，文献涉及ergonomics and anthropometry。这一部分会通过图片和部分文件展示我的数据提取工作流。选3D模型的鼻子数据为例子，因为这个部位与AR眼睛的舒适度紧密相关。
### ***Step 1 手动标点***

### ***Step 2 特征数据***
PCA, 分类，大致的范围

### ***Step 3 平均头模***
在上述的模型中


