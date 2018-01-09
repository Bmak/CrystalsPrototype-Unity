function UnityProgress(dom) {
    this.progress = 0.0;
    this.message = "TEST MESSAGE";
    this.dom = dom;

    var parent = dom.parentNode;

    var bkgImage = document.createElement("img");
    bkgImage.src = "TemplateData/bkg.jpg";
    bkgImage.style.position = "absolute";
    parent.appendChild(bkgImage);
    this.bkgImage = bkgImage;

    var progressFrame = document.createElement("img");
    progressFrame.src = "TemplateData/ProgressFrame.png";
    progressFrame.style.position = "absolute";
    parent.appendChild(progressFrame);
    this.progressFrame = progressFrame;

    var progressBar = document.createElement("div");
    progressBar.style.position = "absolute";
    progressBar.style.overflow = "hidden";
    parent.appendChild(progressBar);
    this.progressBar = progressBar;

    var progressBarImg = document.createElement("img");
    progressBarImg.src = "TemplateData/ProgressBar.png";
    progressBarImg.style.position = "absolute";
    progressBar.appendChild(progressBarImg);
    this.progressBarImg = progressBarImg;

    var messageArea = document.createElement("p");
    messageArea.style.position = "absolute";
    parent.appendChild(messageArea);
    this.messageArea = messageArea;

    this.Clear = function () {
        this.bkgImage.style.display = "none";
        this.progressFrame.style.display = "none";
        this.progressBar.style.display = "none";
        this.messageArea.style.display = "none";
    }

    this.SetProgress = function (progress) {

        if (this.progress < progress)
            this.progress = progress;

        this.messageArea.style.display = "none";
        this.progressFrame.style.display = "inline";
        this.progressBar.style.display = "inline";
        this.messageArea.style.display = "inline";
        this.Update();

    }

    this.SetMessage = function (message) {
        this.message = message;

        this.bkgImage.style.display = "inline";
        this.progressFrame.style.display = "inline";
        this.progressBar.style.display = "inline";
        this.messageArea.style.display = "inline";
        this.Update();
    }

    this.Update = function () {

        this.bkgImage.style.top = this.dom.offsetTop + (this.dom.offsetHeight * 0.5 - bkgImage.height * 0.5) + 'px';
        this.bkgImage.style.left = this.dom.offsetLeft + (this.dom.offsetWidth * 0.5 - bkgImage.width * 0.5) + 'px';
        this.bkgImage.style.width = bkgImage.width + 'px';
        this.bkgImage.style.height = bkgImage.height + 'px';

        var progressFrameImg = new Image();
        progressFrameImg.src = this.progressFrame.src;

        this.progressFrame.style.top = this.dom.offsetTop + (this.dom.offsetHeight * 0.5 + bkgImage.height * 0.35) + 'px';
        this.progressFrame.style.left = this.dom.offsetLeft + (this.dom.offsetWidth * 0.5 - progressFrameImg.width * 0.5) + 'px';
        this.progressFrame.width = progressFrameImg.width;
        this.progressFrame.height = progressFrameImg.height;

        this.progressBarImg.style.top = '0px';
        this.progressBarImg.style.left = '0px';
        this.progressBarImg.width = progressFrameImg.width;
        this.progressBarImg.height = progressFrameImg.height;

        this.progressBar.style.top = this.progressFrame.style.top;
        this.progressBar.style.left = this.progressFrame.style.left;
        this.progressBar.style.width = (progressFrameImg.width * this.progress) + 'px';
        this.progressBar.style.height = progressFrameImg.height + 'px';

        this.messageArea.style.top = this.dom.offsetTop + (this.dom.offsetHeight * 0.5 + bkgImage.height * 0.37) + 'px';
        this.messageArea.style.left = '0px';
        this.messageArea.style.width = '100%';
        this.messageArea.style.textAlign = 'center';
        this.messageArea.innerHTML = this.message;
    }

    this.Update();
}